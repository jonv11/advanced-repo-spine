# ADR-0007: Report and Export Are Aliases for Compare

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-28 |
| **Owner(s) / Deciders** | — |
| **Related RFC** | [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |

---

## Decision

Implement `report` and `export` as ergonomic aliases for `compare --format text` and `compare --format json` respectively, sharing the same comparison engine and producing identical results.

## Context

PRD-0001 defines six CLI commands: `init`, `validate`, `compare`, `report`, `suggest`, and `export`. However, during RFC development (RFC-0001 §"Resolving compare/report/export Overlap"), analysis revealed that `compare`, `report`, and `export` perform the same core operation — run the comparison engine — and differ only in output format. This raises a question: should they be three independent commands with duplicated logic, three commands with shared internals but distinct code paths, or aliases for one command?

PRD-0001 §11 listed this overlap as an open question. This ADR resolves it.

## Options Considered

### Option 1: Aliases for compare with --format flag (Chosen)

**Description:** `compare` is the primary command with a `--format` flag (values: `text`, `json`; default: `text`). `report` is registered as an alias for `compare --format text`, and `export` is registered as an alias for `compare --format json`. All three share a single `CompareCommand` class.

```
ars compare                  # text output (default)
ars compare --format json    # JSON output
ars report                   # alias → compare --format text
ars export                   # alias → compare --format json
```

**Pros:**
- Zero code duplication — one command class, one code path
- Backwards-compatible with PRD — all six command names work
- Users who know `compare` can use flags; users who don't can use memorable aliases
- Easy to test — testing `compare` with both formats covers all three commands
- Adding new formats in v2 requires only adding a format value, not a new command

**Cons:**
- Users may not realize `report` and `compare` are the same operation
- `--help` output must explain the alias relationship clearly
- Spectre.Console.Cli does not have native alias support — aliases must be registered as separate commands pointing to the same handler

### Option 2: Three independent commands

**Description:** `compare`, `report`, and `export` are separate command classes, each implementing the full comparison and formatting pipeline.

**Pros:**
- Each command is self-contained — easy to understand in isolation
- No alias confusion — each command has a single name

**Cons:**
- Code duplication across three commands
- Risk of behavioral divergence (one command gets a bug fix, others don't)
- More commands to test and maintain
- Contradicts the separation of concerns principle

### Option 3: Single command, no aliases

**Description:** Only `compare` exists. `report` and `export` are removed.

```
ars compare                  # text output (default)
ars compare --format json    # JSON output
```

**Pros:**
- Simplest implementation — one command, one name
- No alias confusion
- Smallest CLI surface area

**Cons:**
- Breaks the PRD contract — users and AI agents expect six commands
- `export` is a more discoverable name for JSON output than `compare --format json`
- Removes ergonomic shortcuts that serve different user contexts (CI vs. terminal vs. tooling)

## Rationale

Aliases preserve the intuitive six-command surface defined in the PRD while eliminating code duplication. `report` is the natural command for a human at a terminal who wants a readable summary. `export` is the natural command for a CI pipeline or AI agent that needs machine-readable JSON. Both are thin aliases over the same `compare` engine.

Three independent commands were rejected because duplicated logic is a maintenance and correctness burden — especially since comparison output must be deterministic, and divergent implementations would violate that constraint.

Removing `report` and `export` was rejected because it would break the PRD contract and reduce CLI ergonomics. The command names carry intent: `report` signals "show me results", `export` signals "give me data". These distinctions improve discoverability.

Implementation in Spectre.Console.Cli: both `report` and `export` are registered as separate commands that internally delegate to `CompareCommand` with preset format values. This is a lightweight wrapper — no subclassing or complex routing required.

Deterministic and predictable behavior requires that the `--format` option reject unsupported values with a clear error and non-zero exit code (exit 2) rather than silently defaulting. Valid values are `text` and `json`; any other value is an input error. This fail-fast contract prevents scripts and CI pipelines from receiving unexpected output formats without warning.

## Consequences

### Positive

- Single comparison code path — behavior is guaranteed identical regardless of which alias is invoked
- All six PRD command names are functional — documentation and tutorials remain valid
- Easy to add future formats (e.g., `compare --format markdown`) without new commands
- Testing effort is focused on one command with format variations

### Negative

- `ars --help` shows three commands that do the same thing — must include clear descriptions indicating the alias relationship
- Users may not discover the `--format` flag if they only know `report` or `export`
- Spectre.Console.Cli requires registering aliases as separate commands — minor implementation overhead
- The `--format` option must validate its value and reject unsupported formats with exit code 2 and a message listing valid values, rather than silently coercing to a default

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | Six commands listed in §9; open question about overlap in §11 |
| [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) | Alias resolution in §"Resolving compare/report/export Overlap"; data flow diagrams for compare/report/export |
| [ADR-0006 — Comparison Semantics](ADR-0006-comparison-semantics.md) | Defines the findings that compare/report/export produce |
| [ADR-0004 — v1 Read-Only](ADR-0004-v1-read-only.md) | Compare output is advisory, not actionable — read-only constraint |
