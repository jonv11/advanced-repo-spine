# RFC-0004 — Output Strictness and Invalid Option Handling

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Target Release** | v1 |
| **Owner(s)** | — |
| **Reviewers** | — |
| **Date** | 2026-03-29 |
| **Related PRD** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |
| **Related Links** | [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md), [RFC-0002 — Output/Error Contract](RFC-0002-output-error-contract.md), [ADR-0007 — Report/Export Aliases](../adr/ADR-0007-report-export-aliases.md), [ADR-0006 — Comparison Semantics](../adr/ADR-0006-comparison-semantics.md) |

---

## Summary

This RFC requires that ARS reject invalid option values with a clear error and exit code 2 rather than silently defaulting. The policy applies to all options with a finite valid set: `--format` on `compare`, `report`, `export`, `suggest`, and `validate`. Accepted values are validated case-insensitively. For v1, validation uses string comparison with an explicit helper rather than enum conversion, keeping full control over user-facing error messages. Error output goes to stderr per RFC-0002.

---

## Context / Background

ADR-0007 establishes that `compare`, `report`, and `export` share a single code path with a `--format` flag accepting `text` or `json`. The `suggest` command also accepts `--format`, and RFC-0002 adds `--format` to `validate`.

Currently, the `CompareCommand` implementation uses a string match to select the output formatter. When the format value does not match any known formatter, the code falls through to the default (text) output without warning. This creates a silent failure: the user requests a format, receives a different one, and has no way to detect the mismatch except by inspecting the output.

This behavior is especially harmful in automation, where a script might request `--format json` with a typo (`--format jsn`) and receive text output that breaks the downstream JSON parser — with exit code 0 suggesting success.

ADR-0007 has been amended to explicitly require that unsupported `--format` values be rejected with exit code 2.

---

## Problem Statement

Invalid option values are silently accepted instead of being rejected. This violates the principle that ARS should produce deterministic, predictable output. A tool that silently ignores what the user asked for is worse than one that fails loudly.

---

## Goals

- G-1: Invalid `--format` values produce a clear error message and exit code 2
- G-2: Establish a general policy that options with finite valid value sets are validated before command execution
- G-3: Error messages for invalid options are written to stderr (per RFC-0002)

## Non-Goals

- Suggesting the closest valid value (e.g., "did you mean 'json'?")
- Adding new output formats
- Changing the default format behavior when `--format` is omitted

---

## Requirements

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| R-1 | `--format` must reject values other than `text` and `json` with exit code 2 | Must | ADR-0007 amendment |
| R-2 | Accepted `--format` values must be validated case-insensitively (e.g., `JSON`, `Json`, `json` are all accepted) | Must | Usability |
| R-3 | The error message must name the invalid value and list valid values | Must | Usability |
| R-4 | Validation must occur before command execution (fail-fast) | Must | ADR-0007 rationale |
| R-5 | The error message must go to stderr | Must | RFC-0002 |
| R-6 | The same validation policy applies to any current or future options with finite valid value sets | Must | Consistency |

---

## Proposed Design

### Validation Approach

Use string comparison with an explicit shared helper rather than enum conversion. This keeps the CLI in full control of the user-facing error message, which matters for a tool targeting both humans and AI agents.

A shared helper avoids duplicating validation logic across `compare`, `suggest`, and `validate` (and transitively `report` and `export`, which delegate to `compare`):

```csharp
public static class OptionValidation
{
    private static readonly string[] ValidFormats = { "text", "json" };

    public static bool TryValidateFormat(string value, out string? errorMessage)
    {
        if (ValidFormats.Any(f => f.Equals(value, StringComparison.OrdinalIgnoreCase)))
        {
            errorMessage = null;
            return true;
        }

        errorMessage = $"Unknown format '{value}'. Valid formats: {string.Join(", ", ValidFormats)}";
        return false;
    }
}
```

This is implementation guidance, not a mandatory architecture — the key requirement is that every command with `--format` validates before execution and produces a consistent error. A shared helper is the recommended approach to avoid divergence.

### Error Message Format

```
Error: Unknown format 'xml'. Valid formats: text, json
```

Written to stderr via the error console defined in RFC-0002. Exit code 2 (`ExitCodes.InvalidInput`).

### Commands Affected

| Command | Has `--format` | Validation needed |
|---------|---------------|-------------------|
| `compare` | Yes | Yes — validate before `ExecuteCompare` |
| `report` | No (delegates to compare with `"text"`) | No — hardcoded valid value |
| `export` | No (delegates to compare with `"json"`) | No — hardcoded valid value |
| `suggest` | Yes | Yes — validate before suggestion execution |
| `validate` | Yes (added by RFC-0002) | Yes — validate before model validation |
| `init` | No | No |

### Relationship to RFC-0002

This RFC depends on RFC-0002's stderr contract for error output. The validation policy itself is valid independently — invalid input should be rejected regardless of which channel carries the error — but the specific mechanism (writing to stderr via `ErrorConsole.Stderr`) follows the contract defined in RFC-0002.

---

## Alternatives Considered

### Alternative 1: Warning with fallback to default

**Description:** Accept any format value, but print a warning when the value is unrecognized and fall back to the default (`text`).

**Strengths:** Non-breaking — existing scripts that accidentally pass invalid values continue to work.

**Why not selected:** Silent coercion violates fail-fast. A warning on stderr is better than silence, but still produces output in an unexpected format. Scripts relying on the output format would still break — they just get a warning they may not check.

### Alternative 2: Enum-based format with Spectre validation

**Description:** Change the `Format` property from `string` to an enum (`OutputFormat { Text, Json }`). Spectre.Console.Cli validates enum values automatically.

**Strengths:** Compile-time safety, automatic validation, no custom code needed.

**Why not selected:** Spectre's enum error messages are generic ("Could not convert...") rather than user-friendly. For v1, prefer string + explicit validation so the CLI keeps full control over the user-facing error message. Enum conversion can be reconsidered in a future release if the custom messages prove unnecessary.

---

## Trade-Offs

| Gain | Cost |
|------|------|
| Invalid input is caught immediately | Existing scripts passing invalid values will now fail (intentionally) |
| Users get clear feedback on what went wrong | Small validation helper to maintain |
| Deterministic behavior — output format always matches the request | None significant |
| Shared helper prevents validation divergence across commands | Minor indirection |

---

## Testing / Validation Strategy

- **Unit tests:** Verify that `--format xml`, `--format yaml`, `--format ""` produce exit code 2 with an error message naming the invalid value and listing valid values
- **Unit tests:** Verify that `--format JSON`, `--format Json`, `--format json` are all accepted (case-insensitive)
- **Unit tests:** Verify error message goes to stderr and stdout is empty for invalid format
- **Integration test:** Run `ars compare --format xml --model valid.json` and verify stderr contains error, stdout is empty, exit code is 2

---

## Open Questions (Resolved)

- [x] **Should format validation be case-insensitive?** Yes. Accept `JSON`, `Json`, `json`, etc.
- [x] **Should validation be a shared helper or inline?** Shared helper recommended to prevent divergence across commands. This is implementation guidance, not mandatory architecture.
- [x] **Is the enum approach preferred for type safety?** No, not for v1. Prefer string + explicit validation for full control over error messages. Enum conversion can be reconsidered later.

---

## Decision Outcome / Next Steps

This RFC is accepted. Implementation should:

1. Add a format validation helper (recommended: `OptionValidation.TryValidateFormat`)
2. Call the validation at the start of `CompareCommand.ExecuteCompare`, `SuggestCommand.Execute`, and `ValidateCommand.Execute`
3. On failure: write error to stderr via `ErrorConsole.Stderr`, return `ExitCodes.InvalidInput`
4. Accept format values case-insensitively

---

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | Deterministic output requirements (§20.1) |
| [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md) | Command settings and format flag design |
| [RFC-0002 — Output/Error Contract](RFC-0002-output-error-contract.md) | Defines stderr contract used for error messages |
| [RFC-0003 — CLI Ergonomics](RFC-0003-cli-ergonomics.md) | Defines help text that enumerates valid values; complements this RFC's enforcement |
| [ADR-0006 — Comparison Semantics](../adr/ADR-0006-comparison-semantics.md) | Deterministic output ordering contract |
| [ADR-0007 — Report/Export Aliases](../adr/ADR-0007-report-export-aliases.md) | Defines `--format` valid values; amended to require strict validation |
