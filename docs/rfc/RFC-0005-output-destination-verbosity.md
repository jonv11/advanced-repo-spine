# RFC-0005 — Output Destination and Runtime Verbosity Model

| Field | Value |
|-------|-------|
| **Status** | Draft |
| **Owner(s)** | — |
| **Reviewers** | — |
| **Date** | 2026-03-29 |
| **Related PRD** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |
| **Related Links** | [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md), [RFC-0002 — Output/Error Contract](RFC-0002-output-error-contract.md), [ADR-0004 — v1 Read-Only](../adr/ADR-0004-v1-read-only.md) |

---

## Summary

This RFC proposes adding an `--output <file>` option for writing command results directly to a file and a verbosity model (`--quiet`, `--verbose`) for controlling diagnostic output. These are convenience features identified during black-box testing that improve scripting ergonomics and debugging workflows without changing core command behavior. Both features interact with the output channel contract defined in RFC-0002 and must respect the read-only invariant from ADR-0004 (writing result files is not repository mutation).

---

## Context / Background

ARS v1 sends all output to stdout. Users who want results in a file must use shell redirection (`ars export > results.json`). This works but has limitations: redirection behavior varies across shells, error messages on stdout contaminate the file (addressed separately in RFC-0002), and the CLI cannot confirm where output was written.

Similarly, there is no way to suppress output for scripts that only care about the exit code, or to increase output for debugging model loading, scan behavior, or match resolution. A verbosity model would serve both use cases.

---

## Problem Statement

1. **No file output destination:** Users must rely on shell redirection, which is fragile when combined with error output on stdout (pre-RFC-0002) and varies across platforms.
2. **No verbosity control:** Scripts that only care about pass/fail must still consume and discard output. Users debugging unexpected comparison results have no way to see intermediate matching decisions.

---

## Goals

- G-1: Define an `--output <file>` option that writes command results to a file instead of stdout
- G-2: Define a verbosity model with at least quiet and verbose levels
- G-3: Ensure `--output` does not conflict with the read-only design principle (writing results is not repo mutation)
- G-4: Ensure verbose output goes to stderr so stdout remains pipeable

## Non-Goals

- Structured log files or logging frameworks
- Log rotation or log levels beyond quiet/normal/verbose
- Implementing these features in v1 if scope does not permit — this RFC captures the design for when they are prioritized

---

## Requirements

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| R-1 | `--output <file>` writes command results to the specified file instead of stdout | Should | IDEA-002 |
| R-2 | When `--output` is used, a confirmation message is written to stderr (e.g., "Results written to output.json") | Should | Usability |
| R-3 | `--output` must refuse to overwrite an existing file unless `--force` is also specified, or the behavior must be explicitly defined | Should | Safety |
| R-4 | `--quiet` suppresses all normal output; only exit code signals the result | Could | IDEA-003 |
| R-5 | `--verbose` adds diagnostic output (model load path, scan timing, match details) to stderr | Could | IDEA-003 |
| R-6 | Verbose output must go to stderr, not stdout | Must (if implemented) | RFC-0002 alignment |

---

## Proposed Design

### `--output <file>`

Add a global option `--output <file>` (short form: `-o`) available on `compare`, `report`, `export`, and `suggest`. When specified:

1. Normal result output is written to the file instead of stdout.
2. A confirmation message is written to stderr: `Results written to <file>`.
3. Error output still goes to stderr (per RFC-0002).
4. If the file already exists, the command exits with code 2 and an error message unless `--force` is also specified.

The `--output` option writes a result artifact, not a repository file. This is consistent with ADR-0004's read-only constraint: the tool is still not modifying the user's repository structure.

### Verbosity Model

Add a global `--verbosity` option with three levels:

| Level | Flag | Behavior |
|-------|------|----------|
| Quiet | `--quiet` or `--verbosity quiet` | Suppress all stdout output. Exit code only. Errors still go to stderr. |
| Normal | (default) | Current behavior. |
| Verbose | `--verbose` or `--verbosity verbose` | Add diagnostic details to stderr: model path resolved, items loaded, scan root, items scanned, scan duration, match resolution steps. |

`--quiet` and `--verbose` are mutually exclusive shorthand flags.

### Interaction Matrix

| Combination | stdout | stderr |
|-------------|--------|--------|
| (default) | Results | Errors |
| `--quiet` | (empty) | Errors |
| `--verbose` | Results | Errors + diagnostics |
| `--output file` | (empty) | Confirmation + errors |
| `--output file --verbose` | (empty) | Confirmation + diagnostics + errors |
| `--output file --quiet` | (empty) | Errors only (no confirmation) |

---

## Alternatives Considered

### Alternative 1: No `--output` — rely on shell redirection

**Description:** Keep the current approach where users use `> file` to redirect output.

**Strengths:** Zero implementation effort. Works today.

**Why not selected:** Shell redirection does not provide overwrite safety, does not produce confirmation messages, and behaves differently across shells (PowerShell vs. bash vs. cmd). With RFC-0002's stderr separation, redirection becomes more viable, but `--output` is still a cleaner UX.

### Alternative 2: Single `--verbosity <level>` without shorthand flags

**Description:** Only support `--verbosity quiet|normal|verbose`, no `--quiet`/`--verbose` shorthands.

**Strengths:** Simpler option surface — one flag instead of three.

**Why not selected:** `--quiet` and `--verbose` are widely expected CLI conventions. Requiring `--verbosity quiet` is unnecessarily verbose compared to `--quiet`.

---

## Trade-Offs

| Gain | Cost |
|------|------|
| File output without shell-specific redirection | New option to implement and test; overwrite safety logic |
| Scripts can suppress output with `--quiet` | Option interaction matrix adds complexity |
| Debugging with `--verbose` without code changes | Must define what diagnostics to show; maintenance burden |
| Consistent with common CLI conventions | More options in help text |

---

## Testing / Validation Strategy

- **Unit tests:** Verify `--output file` writes to file and produces confirmation on stderr
- **Unit tests:** Verify `--output` to existing file without `--force` returns exit code 2
- **Unit tests:** Verify `--quiet` produces empty stdout
- **Unit tests:** Verify `--verbose` produces diagnostic output on stderr
- **Integration tests:** Verify `--output file --format json` writes valid JSON to the file
- **Integration tests:** Verify `--quiet` combined with various exit codes (0, 1, 2)

---

## Open Questions

- [ ] Should `--output` default to overwrite or require `--force`? Requiring `--force` is safer but adds friction.
- [ ] Should `--output` be available on `init`? `init` already has `--path` for specifying the output location.
- [ ] What specific diagnostic information should `--verbose` include? This needs a defined list to avoid scope creep.
- [ ] Should these features target v1.1 or v2?

---

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | Output requirements, cross-platform constraints |
| [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md) | Command structure and option design |
| [RFC-0002 — Output/Error Contract](RFC-0002-output-error-contract.md) | Defines stdout/stderr separation that this RFC builds on |
| [ADR-0004 — v1 Read-Only](../adr/ADR-0004-v1-read-only.md) | Read-only constraint — `--output` writes results, not repo mutation |
