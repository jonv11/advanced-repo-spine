# RFC-0002 — Output and Error Channel Contract

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Target Release** | v1 |
| **Owner(s)** | — |
| **Reviewers** | — |
| **Date** | 2026-03-29 |
| **Related PRD** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |
| **Related Links** | [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md), [ADR-0002 — Use Spectre.Console](../adr/ADR-0002-use-spectre-console.md), [ADR-0007 — Report/Export Aliases](../adr/ADR-0007-report-export-aliases.md) |

---

## Summary

This RFC defines the contract for how ARS separates normal output from error/diagnostic output, how it renders dynamic content safely through Spectre.Console, how exit codes map to outcomes, and how structured validation errors are represented in JSON. The exit code mapping (0/1/2/3) is already partially implemented in the codebase via `ExitCodes.cs`; this RFC formalizes and completes the contract. The key trade-off is accepting a dual-console setup (stdout + stderr) over the simpler single-stream approach, which is necessary for reliable automation and piping.

---

## Context / Background

ARS v1 uses `AnsiConsole.MarkupLine()` for both normal output and error messages. This conflates two distinct output channels (results vs. diagnostics) onto stdout and subjects all dynamic content to Spectre.Console's markup parser. The current behavior creates three problems:

1. **Crash on dynamic content:** When validation error locations (e.g., `version`, `items[0].path`) are interpolated into markup strings wrapped in `[...]`, Spectre interprets them as style tags and throws an exception. This produces exit code -1 (framework error) instead of exit code 2 (invalid input), making the `validate` command unusable for its primary purpose.

2. **No channel separation:** Error messages appear on stdout alongside normal output. Scripts piping `ars export` output to a JSON processor receive error text mixed into the data stream if an error occurs.

3. **No structured errors:** Validation failures, file-not-found errors, and other diagnostics are available only as human-readable text. CI/CD pipelines and AI agents cannot programmatically parse error details.

The codebase already defines exit codes in `ExitCodes.cs` (`Success = 0`, `StructuralIssues = 1`, `InvalidInput = 2`, `InternalError = 3`) and uses `Markup.Escape()` for dynamic content in several commands. This RFC formalizes the full contract and addresses the remaining gaps.

ADR-0002 has been amended to require safe rendering of dynamic content through Spectre.Console (bracket escaping or non-markup output methods).

---

## Problem Statement

ARS lacks a defined contract for which output goes where, how errors are rendered safely, and what exit codes mean. This makes the CLI unreliable for automation: scripts cannot separate errors from results, framework exceptions produce undocumented exit codes, and dynamic content can crash the renderer.

---

## Goals

- G-1: Define a clear separation between result output (stdout) and error/diagnostic output (stderr)
- G-2: Define a complete exit code contract covering all command outcomes
- G-3: Eliminate crash paths caused by Spectre markup interpretation of dynamic content
- G-4: Define a minimal structured JSON error contract for validation errors

## Non-Goals

- Implementing logging infrastructure or structured logging to files
- Changing the format or content of normal comparison/report output
- Adding telemetry or observability beyond exit codes
- Defining error codes beyond exit codes (e.g., error catalogs)
- Defining verbose or stack-trace output policy (deferred to RFC-0005 or a future verbosity RFC)

---

## Requirements

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| R-1 | Error and diagnostic messages must be written to stderr, not stdout | Must | PRD-0001 §12 |
| R-2 | Exit codes must be consistent and documented: 0 = success, 1 = structural differences found, 2 = invalid input/usage error, 3 = internal error | Must | PRD-0001 §18 |
| R-3 | Dynamic content rendered through Spectre markup must be bracket-escaped or output via non-markup methods | Must | ADR-0002 amendment |
| R-4 | Framework-level exceptions must not produce undocumented exit codes | Must | PRD-0001 §18 |
| R-5 | When `validate --format json` is specified, structured validation failures must be emitted as JSON on stdout with exit code 2 | Must | PRD-0001 §12.3 |
| R-6 | Normal command output (comparison results, suggestions) goes to stdout | Must | PRD-0001 §12 |
| R-7 | `init` success confirmation is a diagnostic message and must go to stderr | Must | Channel contract consistency |
| R-8 | In non-JSON modes, user-facing errors go to stderr as human-readable text | Must | Channel contract consistency |

---

## Constraints / Invariants

### Constraints

- Spectre.Console is the rendering framework (ADR-0002) — the solution must work within its API
- v1 is read-only except `init` (ADR-0004) — this RFC affects output behavior, not filesystem behavior
- The CLI must degrade gracefully in non-interactive terminals (CI environments)

### Invariants

- **stdout purity:** When a command fails, stdout must be empty (in text mode) or contain a valid JSON error envelope (in JSON mode). Error text must never be mixed into stdout alongside result data.
- **Exit code consistency:** Exit code semantics are the same across all six commands. The mapping is exhaustive — every code path must resolve to one of {0, 1, 2, 3}.
- **Successful commands produce no stderr output.** Diagnostic and confirmation messages on stderr are permitted only for `init` (which has no result payload on stdout) and for future verbosity modes (out of scope for this RFC — see RFC-0005).
- **No silent fallback.** If a command cannot produce the requested output format, it must fail with exit code 2 rather than silently producing a different format (see RFC-0004).

---

## Proposed Design

### Output Channel Separation

Create a static error-console accessor that targets stderr. For v1, a static helper is sufficient; full DI infrastructure is not warranted for this single concern and would conflict with the project's minimal-design principles.

```csharp
public static class ErrorConsole
{
    private static readonly IAnsiConsole Instance = AnsiConsole.Create(
        new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(Console.Error)
        });

    public static IAnsiConsole Stderr => Instance;
}
```

**Usage rules:**

| Output category | Channel | Method |
|----------------|---------|--------|
| Command results (comparison, suggestions, export) | stdout | `AnsiConsole.MarkupLine()` / `Console.WriteLine()` |
| Errors (invalid input, file not found, validation failures in text mode) | stderr | `ErrorConsole.Stderr.MarkupLine()` |
| `init` success confirmation | stderr | `ErrorConsole.Stderr.MarkupLine()` |
| Structured JSON errors (`validate --format json` failures) | stdout | `Console.WriteLine()` with serialized JSON |

### Safe Markup Rendering

All dynamic string values interpolated into Spectre markup must use `Markup.Escape()`:

```csharp
ErrorConsole.Stderr.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
```

The codebase already uses `Markup.Escape()` in several commands. This RFC formalizes the requirement that all dynamic content in markup strings must be escaped.

### Exit Code Contract

| Exit Code | Constant | Meaning | When |
|-----------|----------|---------|------|
| 0 | `ExitCodes.Success` | Success | Command completed normally; no structural differences found (`compare`); model valid (`validate`); file created (`init`); suggestion returned (`suggest`) |
| 1 | `ExitCodes.StructuralIssues` | Structural differences found | `compare`/`report`/`export` found missing or misplaced items |
| 2 | `ExitCodes.InvalidInput` | Invalid input or usage error | Invalid model, file not found, invalid CLI arguments, unsupported option values (see RFC-0004) |
| 3 | `ExitCodes.InternalError` | Internal error | Unexpected exceptions, framework errors |

The top-level exception handler in `Program.cs` must catch unhandled exceptions and return exit code 3 with a diagnostic message on stderr. This is already partially in place and must be formalized.

### Structured Validation Error Contract

When `validate --format json` is specified and the model has validation errors, the command must emit a JSON error envelope on stdout and return exit code 2.

**Schema:**

```json
{
  "success": false,
  "errors": [
    {
      "code": "MISSING_FIELD",
      "message": "Model version is required.",
      "location": "version"
    }
  ]
}
```

**Field definitions:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `success` | `boolean` | Yes | Always `false` for error responses; `true` when the model is valid |
| `errors` | `array` | Yes | List of validation errors (empty when `success` is `true`) |
| `errors[].code` | `string` | Yes | Stable machine-usable error code (e.g., `MISSING_FIELD`, `INVALID_VALUE`, `DUPLICATE_PATH`, `INVALID_PATH_FORMAT`) |
| `errors[].message` | `string` | Yes | Human-readable error description |
| `errors[].location` | `string` | No | JSON path or field name where the error was detected (e.g., `version`, `items[0].path`). Omitted when not applicable. |

**When the model is valid:**

```json
{
  "success": true,
  "errors": []
}
```

The exit code remains 0 in this case.

**Scope:** This structured error contract applies only to `validate --format json` in v1. Other commands emit errors on stderr as human-readable text. Extending structured errors to other commands is a future concern.

### `validate --format` Interaction

The `validate` command does not currently accept `--format`. This RFC requires adding `--format text|json` to `validate`:

- `--format text` (default): Validation errors are written to stderr as human-readable text. Success confirmation is written to stderr.
- `--format json`: Validation result (success or failure) is written to stdout as the JSON envelope defined above. No output on stderr.

This is consistent with the pattern established by `compare --format json`, where the output format flag controls the shape and channel of the primary output.

---

## Alternatives Considered

### Alternative 1: Errors on stdout with a prefix convention

**Description:** Keep all output on stdout but prefix error lines with `ERROR:` so consumers can filter them.

**Strengths:** No API changes, no dual-console setup.

**Why not selected:** Breaks JSON output — a JSON consumer cannot distinguish error text from JSON data without parsing. Violates Unix convention and makes piping unreliable.

### Alternative 2: Dependency injection for error console

**Description:** Use constructor injection to provide the error console to each command, enabling full testability through interface substitution.

**Strengths:** Maximum testability, DI-aligned.

**Why not selected:** Over-engineering for v1. The static helper is testable via stderr stream capture. DI can be introduced later if the codebase grows to warrant it. This aligns with the project's minimal-design principles.

---

## Trade-Offs

| Gain | Cost |
|------|------|
| Scripts can reliably pipe stdout to processors | Requires static error-console accessor and consistent usage |
| Exit codes are predictable and documented | Must audit all command paths for consistent exit code usage |
| Dynamic content cannot crash the renderer | `Markup.Escape()` must be used for all dynamic values in markup strings |
| Structured validation errors enable CI/CD integration | JSON error schema adds a small maintenance surface |
| Static accessor keeps v1 simple | Less flexible than DI; acceptable for v1 scope |

---

## Testing / Validation Strategy

- **Unit tests:** Verify that each command writes errors to stderr (capture stderr stream in tests) and results to stdout
- **Unit tests:** Verify exit code mapping for each outcome (success, differences, invalid input, internal error)
- **Unit tests:** Verify `validate --format json` produces the correct JSON envelope for both valid and invalid models
- **Integration tests:** Pipe `ars export` output through a JSON parser with an invalid model — verify stderr contains the error and stdout is empty
- **Regression tests:** Validate that all previously-crashing dynamic content scenarios (error locations like `version`, `items[0].path`, paths with brackets) render without exceptions
- **Acceptance tests:** Run commands in a CI-like environment (non-interactive terminal) and verify output channel separation

---

## Open Questions (Resolved)

All open questions from the draft have been resolved:

- [x] **Should `validate --format json` produce structured error JSON on stdout?** Yes. When `--format json` is explicitly requested, structured validation failures are emitted on stdout as a JSON envelope. Exit code remains 2 for invalid models, 0 for valid models. In text mode, errors go to stderr.
- [x] **Should the exception handler log stack traces in verbose mode?** This RFC does not define verbose/stack-trace policy. Verbosity behavior is deferred to RFC-0005 or a future dedicated verbosity RFC.
- [x] **Should `init` confirmation messages go to stdout or stderr?** stderr. The `init` confirmation ("Created ars.json") is a diagnostic/confirmation message, not a command result payload. Writing it to stderr keeps stdout clean for potential future piping scenarios.

---

## Decision Outcome / Next Steps

This RFC is accepted. Implementation should:

1. Introduce a static `ErrorConsole` accessor targeting stderr
2. Migrate all error output from `AnsiConsole` (stdout) to `ErrorConsole.Stderr`
3. Move `init` success confirmation to stderr
4. Add `--format text|json` to `validate` command
5. Implement the structured JSON error envelope for `validate --format json`
6. Ensure the top-level exception handler in `Program.cs` maps to exit code 3 with a stderr message
7. Verify all six commands against the exit code contract

---

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | Exit code requirements (§18), output format requirements (§12) |
| [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md) | Defines the command flow and reporting layer |
| [RFC-0004 — Output Strictness](RFC-0004-output-strictness.md) | Defines invalid option value handling; depends on RFC-0002's stderr contract |
| [RFC-0005 — Output Destination and Quiet Mode](RFC-0005-output-destination-verbosity.md) | Verbosity and `--verbose` deferred from this RFC to RFC-0005 or a future RFC |
| [ADR-0002 — Use Spectre.Console](../adr/ADR-0002-use-spectre-console.md) | Framework choice; amended with safe rendering constraint |
| [ADR-0007 — Report/Export Aliases](../adr/ADR-0007-report-export-aliases.md) | Defines format flag semantics and alias behavior |
