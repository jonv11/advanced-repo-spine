# RFC-0004 — Comparison Output Strictness and Invalid Option Handling

| Field | Value |
|-------|-------|
| **Status** | Draft |
| **Owner(s)** | — |
| **Reviewers** | — |
| **Date** | 2026-03-29 |
| **Related PRD** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |
| **Related Links** | [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md), [ADR-0007 — Report/Export Aliases](../adr/ADR-0007-report-export-aliases.md), [ADR-0006 — Comparison Semantics](../adr/ADR-0006-comparison-semantics.md) |

---

## Summary

This RFC proposes that ARS reject invalid option values with a clear error and non-zero exit code rather than silently defaulting. Black-box testing found that `ars compare --format xml` silently produces text output with exit code 0, giving no indication that the requested format was not applied. This violates the fail-fast principle and deterministic behavior contract established by ADR-0006 and ADR-0007. The fix is narrow — validate `--format` before executing the command — but the policy is cross-cutting: all option values with a finite valid set should be validated upfront.

---

## Context / Background

ADR-0007 establishes that `compare`, `report`, and `export` share a single code path with a `--format` flag accepting `text` or `json`. ADR-0007 has been amended to explicitly require that unsupported `--format` values be rejected with exit code 2.

Currently, the `CompareCommand` implementation uses a string match to select the output formatter. When the format value does not match any known formatter, the code falls through to the default (text) output without warning. This creates a silent failure: the user requests a format, receives a different one, and has no way to detect the mismatch except by inspecting the output.

This behavior is especially harmful in automation, where a script might request `--format json` with a typo (`--format jsn`) and receive text output that breaks the downstream JSON parser — with exit code 0 suggesting success.

---

## Problem Statement

Invalid option values are silently accepted instead of being rejected. This violates the principle that ARS should produce deterministic, predictable output. A tool that silently ignores what the user asked for is worse than one that fails loudly.

---

## Goals

- G-1: Invalid `--format` values produce a clear error message naming the valid values and exit code 2
- G-2: Establish a general policy that options with finite valid value sets are validated before command execution
- G-3: Error messages for invalid options are written to stderr (consistent with RFC-0002)

## Non-Goals

- Suggesting the closest valid value (e.g., "did you mean 'json'?")
- Adding new output formats
- Changing the default format behavior when `--format` is omitted

---

## Requirements

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| R-1 | `--format` must reject values other than `text` and `json` with exit code 2 | Must | ADR-0007 amendment, QOL-004 finding |
| R-2 | The error message must name the invalid value and list valid values | Must | Usability |
| R-3 | Validation must occur before command execution (fail-fast) | Must | ADR-0007 rationale |
| R-4 | The error message must go to stderr | Should | RFC-0002 |
| R-5 | The same validation pattern should apply to any future options with finite valid value sets | Should | Consistency |

---

## Proposed Design

### Validation Approach

Add format validation at the start of the command's `Execute` method (or in a shared validation helper):

```csharp
var validFormats = new[] { "text", "json" };
if (!validFormats.Contains(settings.Format, StringComparer.OrdinalIgnoreCase))
{
    errorConsole.MarkupLine($"[red]Error:[/] Unknown format '{Markup.Escape(settings.Format)}'. Valid formats: {string.Join(", ", validFormats)}");
    return ExitCodes.InvalidInput; // 2
}
```

### Alternative: Spectre TypeConverter

Use a custom `TypeConverter` on the `Format` property to restrict valid values at the parsing level. This would produce an error before `Execute` is even called, but gives less control over the error message format.

### Error Message Format

```
Error: Unknown format 'xml'. Valid formats: text, json
```

Written to stderr. Exit code 2 (InvalidInput).

---

## Alternatives Considered

### Alternative 1: Warning with fallback to default

**Description:** Accept any format value, but print a warning when the value is unrecognized and fall back to the default (`text`).

**Strengths:** Non-breaking — existing scripts that accidentally pass invalid values continue to work.

**Why not selected:** Silent coercion violates fail-fast. A warning on stderr is better than silence, but still produces output in an unexpected format. Scripts relying on the output format would still break — they just get a warning they may not check.

### Alternative 2: Enum-based format with Spectre validation

**Description:** Change the `Format` property from `string` to an enum (`OutputFormat { Text, Json }`). Spectre.Console.Cli validates enum values automatically.

**Strengths:** Compile-time safety, automatic validation, no custom code needed.

**Why not selected:** This is actually a strong implementation option and could be recommended. The trade-off is that Spectre's enum error messages are generic ("Could not convert...") rather than user-friendly. Could be combined with a custom TypeConverter for better messages.

---

## Trade-Offs

| Gain | Cost |
|------|------|
| Invalid input is caught immediately | Existing scripts passing invalid values will now fail (intentionally) |
| Users get clear feedback on what went wrong | Small validation code to add and maintain |
| Deterministic behavior — output format always matches the request | None significant |

---

## Testing / Validation Strategy

- **Unit tests:** Verify that `--format xml`, `--format yaml`, `--format ""`, and `--format JSON` (case test) produce appropriate behavior (error for invalid, success for valid regardless of case)
- **Unit tests:** Verify exit code 2 for invalid format values
- **Unit tests:** Verify error message contains the invalid value and lists valid values
- **Integration test:** Run `ars compare --format xml --model valid.json` and verify stderr contains error, stdout is empty, exit code is 2

---

## Open Questions

- [ ] Should format validation be case-insensitive (accept `JSON`, `Json`, `json`)?
- [ ] Should the format validation be implemented as a shared helper (for reuse by `suggest --format`) or inline in each command?
- [ ] Is the enum approach (Alternative 2) preferred over string validation for type safety?

---

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | Deterministic output requirements |
| [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md) | Command settings and format flag design |
| [ADR-0006 — Comparison Semantics](../adr/ADR-0006-comparison-semantics.md) | Deterministic output ordering contract |
| [ADR-0007 — Report/Export Aliases](../adr/ADR-0007-report-export-aliases.md) | Defines `--format` valid values; amended to require strict validation |
