# RFC-0005 — Output Destination and Quiet Mode

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Target Release** | v1.x |
| **Owner(s)** | — |
| **Reviewers** | — |
| **Date** | 2026-03-29 |
| **Related PRD** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |
| **Related Links** | [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md), [RFC-0002 — Output/Error Contract](RFC-0002-output-error-contract.md), [ADR-0004 — v1 Read-Only](../adr/ADR-0004-v1-read-only.md) |

---

## Summary

This RFC adds an `--output <file>` option for writing command results directly to a file and an optional `--quiet` flag for suppressing non-error output when only the exit code matters. These are convenience features that improve scripting ergonomics without changing core command behavior. Both features build on the stdout/stderr contract defined in RFC-0002 and respect the read-only invariant from ADR-0004 (writing result files is not repository mutation). This RFC does not target v1 core correctness work; it targets a follow-up release after v1 stabilization.

---

## Context / Background

ARS v1 sends all command result output to stdout. Users who want results in a file must use shell redirection (`ars export > results.json`). This works but has limitations: redirection behavior varies across shells and platforms, and the CLI cannot confirm where output was written.

Similarly, there is no way to suppress output for scripts that only care about the exit code.

RFC-0002 establishes the stdout/stderr separation. This RFC builds on that contract by adding `--output` (redirects stdout result content to a file) and `--quiet` (suppresses non-error stdout output entirely).

---

## Problem Statement

1. **No file output destination:** Users must rely on shell redirection, which varies across platforms and does not provide overwrite safety.
2. **No quiet mode:** Scripts that only care about pass/fail must consume and discard all output.

---

## Goals

- G-1: Define an `--output <file>` option that writes command results to a file instead of stdout
- G-2: Define a `--quiet` flag that suppresses non-error output
- G-3: Ensure `--output` does not conflict with the read-only design principle (writing results is not repo mutation)
- G-4: Ensure both features are compatible with RFC-0002's stdout/stderr contract

## Non-Goals

- Structured log files or logging frameworks
- Verbose/debug output (`--verbose` is explicitly deferred to a future RFC — the diagnostic content, interaction with stderr, and scope are underspecified and would require separate design work)
- Log rotation or log levels
- Implementing these features in v1 core — this RFC targets v1.x

---

## Requirements

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| R-1 | `--output <file>` writes command results to the specified file instead of stdout | Must | PRD-0001 §12 |
| R-2 | When `--output` is used, a confirmation message is written to stderr (e.g., "Results written to output.json") | Must | Usability |
| R-3 | `--output` must refuse to overwrite an existing file unless `--force` is also specified | Must | Safety |
| R-4 | `--quiet` suppresses all non-error stdout output; exit code alone signals the result | Should | Scripting ergonomics |
| R-5 | `--quiet` must never suppress real errors on stderr | Must | Safety |
| R-6 | `--quiet` must not alter the command result payload itself (findings, exit code) | Must | Correctness |
| R-7 | `--output` is not available on `init` (`init --path` already owns the file destination concern) | Must | Scope clarity |

---

## Proposed Design

### `--output <file>`

Add a global option `--output <file>` (short form: `-o`) available on `compare`, `report`, `export`, `suggest`, and `validate`. When specified:

1. Command result output is written to the file instead of stdout.
2. A confirmation message is written to stderr: `Results written to <file>`.
3. Error output goes to stderr as usual (per RFC-0002).
4. If the file already exists, the command returns exit code 2 with an error on stderr unless `--force` is also specified.

`--output` is not available on `init`. The `init` command already uses `--path` to specify its output location, and adding `--output` would create ambiguity between "where to write the model file" and "where to write command output."

`--output` writes a result artifact, not a repository file. This is consistent with ADR-0004's read-only constraint.

### `--force` with `--output`

`--force` on `--output` means "overwrite the output file if it already exists." This is scoped to the `--output` concern and does not overlap with `init --force` (which means "overwrite the model file"). The semantics are distinct because the targets are different.

### `--quiet`

Add a global option `--quiet` (short form: `-q`). When specified:

1. All non-error stdout output is suppressed. The command performs its full operation but does not write results to stdout.
2. Errors on stderr are never suppressed. If the command fails, the error message still appears on stderr.
3. The exit code is unaffected.
4. `--quiet` does not alter the command's internal behavior — comparison still runs, validation still checks all rules. Only output is suppressed.

`--quiet` is defined narrowly: it suppresses non-error diagnostic/confirmation/result output only. It must never suppress real errors and must never change the exit code or the logical result of the command.

### Interaction Matrix

| Combination | stdout | stderr | File |
|-------------|--------|--------|------|
| (default) | Results | Errors only | — |
| `--output file` | (empty) | Confirmation + errors | Results |
| `--quiet` | (empty) | Errors only | — |
| `--output file --quiet` | (empty) | Errors only (no confirmation) | Results |
| `--output file --force` | (empty) | Confirmation + errors | Results (overwritten) |

### `--output` with Text and JSON Modes

`--output` writes whatever the command would have written to stdout. The interaction with `--format` is straightforward:

```
ars compare --model ars.json --format json --output results.json
# Writes JSON comparison results to results.json

ars compare --model ars.json --format text --output results.txt
# Writes text comparison results to results.txt

ars validate --model ars.json --format json --output validation.json
# Writes JSON validation result to validation.json
```

The file receives exactly the same content that would have appeared on stdout.

---

## Alternatives Considered

### Alternative 1: No `--output` — rely on shell redirection

**Description:** Keep the current approach where users use `> file` to redirect output.

**Strengths:** Zero implementation effort. Works today.

**Why not selected:** Shell redirection does not provide overwrite safety, does not produce confirmation messages, and behaves differently across shells (PowerShell vs. bash vs. cmd). `--output` is a cleaner cross-platform UX.

### Alternative 2: Single `--verbosity <level>` with quiet/normal/verbose

**Description:** Implement a full verbosity model covering quiet, normal, and verbose in one RFC.

**Strengths:** Comprehensive, handles all verbosity use cases.

**Why not selected:** `--verbose` is underspecified — the diagnostic content, interaction with stderr, and scope require separate design work. Bundling it with `--output` and `--quiet` creates a broad RFC that cannot be cleanly accepted. `--quiet` can be specified precisely on its own. `--verbose` is deferred to a future RFC.

---

## Trade-Offs

| Gain | Cost |
|------|------|
| File output without shell-specific redirection | New option to implement and test; overwrite safety logic |
| Scripts can suppress output with `--quiet` | Option interaction matrix adds modest complexity |
| Cross-platform consistency | More options in help text |
| `--verbose` deferred — keeps this RFC clean | Verbose/debug use case remains unaddressed until a future RFC |

---

## Testing / Validation Strategy

- **Unit tests:** Verify `--output file` writes results to the specified file and produces confirmation on stderr
- **Unit tests:** Verify `--output` to existing file without `--force` returns exit code 2 with error on stderr
- **Unit tests:** Verify `--output` to existing file with `--force` succeeds and overwrites
- **Unit tests:** Verify `--quiet` produces empty stdout and does not suppress stderr errors
- **Unit tests:** Verify `--quiet` does not change exit codes
- **Integration tests:** Verify `--output file --format json` writes valid JSON to the file
- **Integration tests:** Verify `--quiet` combined with various exit codes (0, 1, 2)
- **Integration tests:** Verify `--output file --quiet` writes file without confirmation on stderr

---

## Open Questions (Resolved)

- [x] **Should `--output` default to overwrite or require `--force`?** Require `--force`. Refusing to overwrite by default is safer and consistent with `init --force` semantics.
- [x] **Should `--output` be available on `init`?** No. `init --path` already owns the file destination concern; adding `--output` would create ambiguity.
- [x] **What about `--verbose`?** Deferred to a future RFC. The diagnostic content is underspecified, overlaps with RFC-0002's error-channel concerns, and cannot be cleanly defined in this RFC's scope.
- [x] **Should these features target v1?** No. This RFC targets v1.x, a follow-up release after v1 core stabilization.

---

## Decision Outcome / Next Steps

This RFC is accepted for v1.x. Implementation should:

1. Add `--output <file>` and `--force` as global options on `compare`, `report`, `export`, `suggest`, `validate`
2. Add `--quiet` as a global option on all commands
3. Implement overwrite protection for `--output`
4. Follow RFC-0002's channel contract for confirmation messages and errors
5. Exclude `--output` from `init`

---

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | Output requirements (§12), cross-platform constraints (§13) |
| [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md) | Command structure and option design |
| [RFC-0002 — Output/Error Contract](RFC-0002-output-error-contract.md) | Defines stdout/stderr separation that this RFC builds on |
| [ADR-0004 — v1 Read-Only](../adr/ADR-0004-v1-read-only.md) | Read-only constraint — `--output` writes results, not repo mutation |
