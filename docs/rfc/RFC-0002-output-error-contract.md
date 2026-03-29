# RFC-0002 — Output and Error Channel Contract

| Field | Value |
|-------|-------|
| **Status** | Draft |
| **Owner(s)** | — |
| **Reviewers** | — |
| **Date** | 2026-03-29 |
| **Related PRD** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |
| **Related Links** | [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md), [ADR-0002 — Use Spectre.Console](../adr/ADR-0002-use-spectre-console.md), [ADR-0007 — Report/Export Aliases](../adr/ADR-0007-report-export-aliases.md) |

---

## Summary

This RFC proposes a formal contract for how ARS separates normal output from error/diagnostic output, how it renders dynamic content safely through Spectre.Console, and how exit codes map to outcomes. Black-box testing exposed three related issues: a critical crash caused by Spectre markup interpreting dynamic error locations as style tags, all error messages being written to stdout instead of stderr, and no machine-readable error format for automation consumers. These are cross-cutting concerns that affect every command and every consumer (human, script, CI pipeline, AI agent).

---

## Context / Background

ARS v1 uses `AnsiConsole.MarkupLine()` for both normal output and error messages. This conflates two distinct output channels (results vs. diagnostics) onto stdout and subjects all dynamic content to Spectre.Console's markup parser. The current behavior creates three problems:

1. **Crash on dynamic content:** When validation error locations (e.g., `version`, `items[0].path`) are interpolated into markup strings wrapped in `[...]`, Spectre interprets them as style tags and throws an exception. This produces exit code -1 (framework error) instead of exit code 2 (invalid input), making the `validate` command unusable for its primary purpose.

2. **No channel separation:** Error messages appear on stdout alongside normal output. Scripts piping `ars export` output to a JSON processor receive error text mixed into the data stream if an error occurs.

3. **No structured errors:** Validation failures, file-not-found errors, and other diagnostics are available only as human-readable text. CI/CD pipelines and AI agents cannot programmatically parse error details.

ADR-0002 has been amended to require safe rendering of dynamic content through Spectre.Console (bracket escaping or non-markup output methods). This RFC addresses the broader output architecture.

---

## Problem Statement

ARS lacks a defined contract for which output goes where, how errors are rendered safely, and what exit codes mean. This makes the CLI unreliable for automation: scripts cannot separate errors from results, framework exceptions produce undocumented exit codes, and dynamic content can crash the renderer.

---

## Goals

- G-1: Define a clear separation between result output (stdout) and error/diagnostic output (stderr)
- G-2: Define a complete exit code contract covering all command outcomes
- G-3: Eliminate crash paths caused by Spectre markup interpretation of dynamic content
- G-4: Evaluate structured (JSON) error output for automation consumers

## Non-Goals

- Implementing logging infrastructure or structured logging to files
- Changing the format or content of normal comparison/report output
- Adding telemetry or observability beyond exit codes
- Defining error codes beyond exit codes (e.g., error catalogs)

---

## Requirements

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| R-1 | Error and diagnostic messages must be written to stderr, not stdout | Must | QOL-002 finding |
| R-2 | Exit codes must be consistent and documented: 0 = success, 1 = comparison found differences, 2 = invalid input/usage error, 3+ = internal error | Must | BUG-001 finding, PRD-0001 §17 |
| R-3 | Dynamic content rendered through Spectre markup must be bracket-escaped or output via non-markup methods | Must | BUG-001 finding, ADR-0002 amendment |
| R-4 | Framework-level exceptions must not produce undocumented exit codes | Must | BUG-001 finding |
| R-5 | A `--format json` mode for error output should be considered for validation errors so automation consumers can parse failure details | Should | IDEA-001 |
| R-6 | Normal command output (comparison results, suggestions, init confirmations) remains on stdout | Must | Existing behavior to preserve |

---

## Constraints / Invariants

### Constraints

- Spectre.Console is the rendering framework (ADR-0002) — the solution must work within its API
- v1 is read-only except `init` (ADR-0004) — this RFC affects output behavior, not filesystem behavior
- The CLI must degrade gracefully in non-interactive terminals (CI environments)

### Invariants

- Normal command output on stdout must not change format or content due to this RFC
- Exit code semantics must be consistent across all six commands
- A command that succeeds must never produce output on stderr (beyond optional diagnostics in verbose mode, if added later)

---

## Proposed Design

### Output Channel Separation

Create a dedicated error console targeting stderr:

```csharp
var errorConsole = AnsiConsole.Create(new AnsiConsoleSettings
{
    Out = new AnsiConsoleOutput(Console.Error)
});
```

Use `errorConsole` for all error and diagnostic output. Use the default `AnsiConsole` (stdout) for normal command results. Pass both consoles to command implementations via dependency injection or a shared context object.

### Safe Markup Rendering

All dynamic string values interpolated into Spectre markup must follow one of two patterns:

1. **Bracket-escape literal brackets:** `[[{Markup.Escape(value)}]]` when square brackets are intended as display characters around dynamic content.
2. **Use non-markup output:** `AnsiConsole.WriteLine()` or string concatenation with `AnsiConsole.Markup()` for the styled prefix only.

### Exit Code Contract

| Exit Code | Meaning | When |
|-----------|---------|------|
| 0 | Success | Command completed normally, no differences found (compare), model valid (validate), file created (init) |
| 1 | Differences found | Compare/report/export found structural differences between model and repo |
| 2 | Invalid input | Invalid model, file not found, invalid CLI arguments, unsupported option values |
| 3 | Internal error | Unexpected exceptions, framework errors |

Framework exceptions (including Spectre rendering errors) must be caught at the top level and mapped to exit code 3 with a diagnostic message on stderr.

### Structured Error Output (Evaluation)

For `validate`, when `--format json` is specified, validation errors could be returned as a JSON array on stdout:

```json
{
  "success": false,
  "errors": [
    { "location": "version", "message": "Model version is required." }
  ]
}
```

This extends the existing `--format json` flag to error paths. The exit code remains 2 (invalid input) regardless of output format. This is a **should** requirement — the exact schema and interaction with other commands should be resolved during review.

---

## Alternatives Considered

### Alternative 1: Errors on stdout with a prefix convention

**Description:** Keep all output on stdout but prefix error lines with `ERROR:` so consumers can filter them.

**Strengths:** No API changes, no dual-console setup.

**Why not selected:** Breaks JSON output — a JSON consumer cannot distinguish error text from JSON data without parsing. Also violates Unix convention and makes piping unreliable.

### Alternative 2: Errors only via exit codes, no stderr text

**Description:** Suppress error messages entirely; rely on exit codes alone for error signaling.

**Strengths:** Simplest implementation, no channel confusion.

**Why not selected:** Unusable for humans — a non-zero exit code without an explanation forces users to guess what went wrong. Also prevents structured error output for automation.

---

## Trade-Offs

| Gain | Cost |
|------|------|
| Scripts can reliably pipe stdout to processors | Requires dual-console setup in Spectre.Console |
| Exit codes are predictable and documented | Must audit all command paths for consistent exit code usage |
| Dynamic content cannot crash the renderer | Developers must remember to escape or use non-markup output for dynamic values |
| Structured errors enable CI/CD integration | JSON error schema must be defined and maintained |

---

## Testing / Validation Strategy

- **Unit tests:** Verify that each command writes errors to stderr (capture stderr stream in tests) and results to stdout
- **Unit tests:** Verify exit code mapping for each outcome (success, differences, invalid input, internal error)
- **Integration tests:** Pipe `ars export` output through a JSON parser with an invalid model — verify stderr contains the error and stdout is empty or contains valid JSON
- **Regression tests:** Validate that all previously-crashing dynamic content scenarios (error locations like `version`, `items[0].path`, paths with brackets) render without exceptions
- **Acceptance tests:** Run commands in a CI-like environment (non-interactive terminal) and verify output channel separation

---

## Open Questions

- [ ] Should `--format json` on `validate` produce structured error JSON on stdout (with exit code 2), or should structured errors always go to stderr?
- [ ] Should the top-level exception handler log stack traces to stderr in a debug/verbose mode, or only the user-facing message?
- [ ] Should `init` confirmation messages ("Created ars.json") go to stdout or stderr? They are not command "results" but are useful for scripting.

---

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | Exit code requirements (§17), output format requirements |
| [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md) | Defines the command flow and reporting layer |
| [ADR-0002 — Use Spectre.Console](../adr/ADR-0002-use-spectre-console.md) | Framework choice; amended with safe rendering constraint |
| [ADR-0007 — Report/Export Aliases](../adr/ADR-0007-report-export-aliases.md) | Defines format flag semantics and alias behavior |
