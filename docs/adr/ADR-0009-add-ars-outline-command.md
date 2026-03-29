# ADR-0009: Add `ars outline` as a New Command

| Field | Value |
|-------|-------|
| **Status** | Proposed |
| **Date** | 2026-03-29 |
| **Owner(s) / Deciders** | — |
| **Related RFC** | [RFC-0007 — `ars outline` Command Design](../rfc/RFC-0007-ars-outline-command-design.md) |
| **Related Links** | [PRD-0002 — Repository Documentation Outline View](../prd/PRD-0002-repository-documentation-outline-view.md), [ADR-0004 — Keep v1 Read-Only Except for `init`](ADR-0004-keep-v1-readonly.md), [ADR-0006 — Define Comparison Semantics](ADR-0006-comparison-semantics.md), [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) |

---

## Decision

Add `ars outline` as a new first-class ARS discovery command alongside `init`, `validate`, `compare`, `report`, `suggest`, and `export`.

## Context

PRD-0002 defines a documentation discovery gap: ARS helps users understand repository path structure but does not expose the internal heading structure of Markdown files. Addressing this gap requires a new user-facing capability.

The question is not whether to add the capability — PRD-0002 is accepted — but how to surface it: as a new command, as a mode on an existing command, or as a deferred feature. Each option has different consequences for command semantics, discoverability, and maintainability.

## Options Considered

### Option 1: Standalone `ars outline` command (Chosen)

**Description:** Register `ars outline` as an independent command in `Program.cs` with its own `Settings` class, following the same registration pattern as the six existing commands.

**Pros:**
- Noun-style naming (`ars outline`) is consistent with the existing command vocabulary (RFC-0001)
- Independently discoverable in `ars --help` with its own description and option listing
- Adding, changing, or deprecating `ars outline` has no effect on the semantics of existing commands
- RFC-0003 help-output requirements apply uniformly — no special cases needed

**Cons:**
- Adds an eighth command to the `ars --help` listing

### Option 2: Add `--headings` mode to `compare` or `report`

**Description:** Extend `CompareCommand` or `ReportCommand` with a flag that switches the output from model-diff results to a heading-expanded tree.

**Pros:**
- Keeps the top-level command count lower
- Heading data co-located with existing output pathways

**Cons:**
- `compare` and `report` are defined by their model-vs-filesystem diff semantics (ADR-0006); adding a traversal-only mode without a model would require either loading a model unnecessarily or introducing a conditional code path that weakens the ADR-0006 invariant
- Bolted-on modes are harder to discover than dedicated commands — users learn one command and may never find the other mode
- PRD-0002 §Out of Scope explicitly states "extending `compare`, `report`, or `export` to become outline commands" is out of scope

### Option 3: Defer to a v2 separate tool or plugin

**Description:** Treat outline capability as a future scope item, either for a future ARS major version or as a separately distributed CLI tool.

**Pros:**
- Keeps the current command surface exactly as-is

**Cons:**
- PRD-0002 is additive and read-only — it does not trigger any v1 constraint (ADR-0004 allows any number of read-only commands)
- ARS's mission is to serve as "the structural backbone for repositories built by humans and AI" — deferring a key discovery capability weakens that mission without a technical reason to do so
- Downstream tooling and AI agents have no stable outline contract until the feature ships

## Rationale

A standalone command is the correct surface for this capability for three reasons:

First, it preserves the semantic clarity of existing commands. `compare` answers "does the repo match the model?" and `report` presents the answer — neither question has anything to do with documentation heading structure. Mixing these concerns would require either loading a model when no diff is needed or branching on a mode flag.

Second, CLI discoverability is a stated design value (RFC-0003). A dedicated command with a clear name (`ars outline`) is found immediately in `ars --help`. A hidden mode on `compare` requires users to read the full option list of an unrelated command.

Third, the capability is purely additive and read-only. ADR-0004 constrains v1 from mutating files; it places no limit on adding new read-only commands. The cost — one more `--help` entry — is negligible.

## Consequences

### Positive

- `ars outline` is discoverable as a first-class command with its own help section
- Adding, modifying, or deprecating the outline command has no impact on model-diff commands
- `compare`, `report`, and `export` retain their existing semantics without branching logic
- The command pattern (`Command<Settings>` registration) is well-established and requires no framework changes

### Negative

- The `ars --help` listing grows by one entry
- One additional command class to maintain over time

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0002 — Repository Documentation Outline View](../prd/PRD-0002-repository-documentation-outline-view.md) | Product requirements that trigger this decision |
| [RFC-0007 — `ars outline` Command Design](../rfc/RFC-0007-ars-outline-command-design.md) | Design RFC that specifies command behavior |
| [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) | Defines the command registration pattern this decision follows |
| [ADR-0004 — Keep v1 Read-Only Except for `init`](ADR-0004-keep-v1-readonly.md) | Confirms new read-only commands are within scope |
| [ADR-0006 — Define Comparison Semantics](ADR-0006-comparison-semantics.md) | Defines compare/report semantics that `ars outline` must not redefine |
