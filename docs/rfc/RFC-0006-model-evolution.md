# RFC-0006 — Model Evolution Strategy (Post-v1)

| Field | Value |
|-------|-------|
| **Status** | Draft |
| **Owner(s)** | — |
| **Reviewers** | — |
| **Date** | 2026-03-29 |
| **Related PRD** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |
| **Related Links** | [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md), [ADR-0003 — JSON Model Format](../adr/ADR-0003-use-json-model-format.md), [ADR-0005 — v1 Model Schema](../adr/ADR-0005-v1-model-schema.md), [ADR-0006 — Comparison Semantics](../adr/ADR-0006-comparison-semantics.md) |

---

## Summary

This RFC outlines the considerations for evolving the ARS model schema beyond v1, covering two topics: schema versioning and migration tooling, and the potential addition of glob/pattern support in model items. These are post-v1 concerns, but capturing the design space now ensures that v1 decisions do not inadvertently block future evolution. Neither feature is proposed for immediate implementation — this RFC establishes the problem space and design options for future prioritization.

---

## Context / Background

ARS v1 defines a model schema (ADR-0005) with explicit, concrete paths: each model item specifies an exact `path` like `src/Program.cs` or `docs/`. The schema includes a `version` field (currently `"1.0"`) intended to support future evolution, but no migration or diff tooling exists.

Two evolution pressures are anticipated:

1. **Schema changes:** As v1 stabilizes and usage patterns emerge, the model schema will need changes — new fields, restructured items, or changed semantics. Without migration tooling, users must manually update their models or face validation errors after upgrading ARS.

2. **Pattern-based matching:** Large repositories with many similar files (e.g., `src/**/*.cs`) are cumbersome to model with explicit paths. Glob or pattern support would reduce model verbosity but fundamentally changes the comparison semantics from exact matching to pattern matching.

PRD-0001 §23 acknowledges YAML support, enhanced validation, and advanced matching as v2 candidates. This RFC adds schema migration and glob patterns to that list with concrete design considerations.

---

## Problem Statement

1. **No migration path:** When the model schema changes between ARS versions, there is no tooling to detect the version mismatch, explain what changed, or transform the model. Users face opaque validation errors.

2. **Model verbosity:** Explicit paths require listing every modeled file individually. For repositories with hundreds of source files in a consistent structure, this is impractical. Pattern support would allow compact declarations like `src/**/*.cs` to cover many files at once.

---

## Goals

- G-1: Define the design space for model schema versioning and migration
- G-2: Define the design space for glob/pattern support in model items
- G-3: Ensure v1 schema and comparison semantics do not create unnecessary barriers to these future features
- G-4: Capture the trade-offs so future ADRs can record decisions with full context

## Non-Goals

- Implementing migration tooling in v1
- Implementing glob support in v1
- Defining the v2 model schema
- Adding YAML support (separate concern, addressed by ADR-0003)

---

## Requirements

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| R-1 | A future migration tool should be able to detect model schema version from the `version` field | Should | Forward compatibility |
| R-2 | Schema changes should be documented in a version changelog or migration guide | Should | Usability |
| R-3 | Pattern support, if added, must not break existing explicit-path models | Must | Backward compatibility |
| R-4 | Pattern support must define clear semantics for how patterns interact with the comparison engine | Must | ADR-0006 alignment |
| R-5 | The v1 `version` field format must not need to change to accommodate future versioning | Should | Forward compatibility |

---

## Proposed Design

### Topic 1: Schema Versioning and Migration

**Version field semantics:** The v1 model uses `"version": "1.0"` as a string. Future versions increment this (e.g., `"1.1"`, `"2.0"`). Semantic versioning rules: minor versions add backward-compatible fields; major versions may remove or restructure fields.

**Migration tooling options:**

| Approach | Description | Pros | Cons |
|----------|-------------|------|------|
| `ars migrate` command | Dedicated command that reads a model, detects its version, and transforms it to the current schema version | Clear UX, discoverable | Adds a write command (conflicts with read-only principle unless scoped as a model editor) |
| `ars validate --upgrade` flag | Validation reports version mismatches and suggests transformations | Reuses existing command | Overloads validate's purpose |
| External migration scripts | Provide JSON transformation scripts or documentation per version bump | No code changes to ARS | Manual effort for users, error-prone |

**Model diff considerations:** A diff between two model files (e.g., before and after a schema change) could show added/removed/changed items. This is conceptually similar to the existing comparison engine but operates on two models rather than a model and a filesystem.

### Topic 2: Glob/Pattern Support

**Current model item:**
```json
{ "name": "program", "type": "file", "path": "src/Program.cs" }
```

**Possible pattern item:**
```json
{ "name": "source-files", "type": "pattern", "path": "src/**/*.cs", "description": "All C# source files" }
```

**Key design questions:**

1. **Matching semantics:** Does a pattern item count as "matched" when at least one file matches? When all expected files match? How are "extra" files (matching the pattern but not representing anything unexpected) handled?

2. **Interaction with explicit items:** If both `src/Program.cs` (explicit) and `src/**/*.cs` (pattern) exist in the model, which takes priority? Does the pattern match exclude items already matched explicitly?

3. **Comparison findings:** The current finding types (present, missing, extra, misplaced) assume exact-path matching. Pattern matching introduces ambiguity — a file matching a pattern is "expected" but was never individually declared.

4. **Model complexity:** Patterns can overlap, creating ambiguous ownership. Two patterns like `src/**/*.cs` and `src/Services/**/*` could both claim the same file.

**Recommendation:** Pattern support requires a new ADR for comparison semantics (potentially amending ADR-0006) and careful design of the finding model. It should not be retrofitted into the explicit-match engine — it is a distinct matching mode.

---

## Alternatives Considered

### Alternative 1: No schema evolution — freeze at v1

**Description:** Declare the v1 schema final. New features use extension fields or a separate config file.

**Strengths:** Maximum stability. No migration tooling needed.

**Why not selected:** Impractical for a tool expected to evolve. Extension fields become a secondary schema that replicates the problem.

### Alternative 2: Auto-migrate on load

**Description:** The model loader detects the version and silently upgrades the in-memory representation. No user action needed.

**Strengths:** Seamless user experience.

**Why not selected:** Silent mutation of semantics is dangerous — the user's model file stays at the old version while the tool interprets it differently. Explicit migration with user confirmation is safer.

---

## Trade-Offs

| Gain | Cost |
|------|------|
| Users can upgrade ARS without manually rewriting models | Migration tooling must be built and maintained per schema version |
| Pattern support reduces model size for large repos | Comparison semantics become significantly more complex |
| Forward-compatible version field design | Requires discipline to follow semver for schema versions |

---

## Testing / Validation Strategy

- **Schema versioning:** Test that the validator rejects models with unsupported version values and provides a clear error message
- **Migration (when implemented):** Test round-trip migration: v1.0 model → migrate → v1.1 model → validate passes; original model preserved
- **Pattern matching (when implemented):** Test explicit-path and pattern-path interactions; test overlapping patterns; test empty-match patterns; test that existing explicit-path models are unaffected

---

## Open Questions

- [ ] Should the `version` field use semver strings (`"1.0.0"`) or simple major.minor (`"1.0"`)? Current v1 uses `"1.0"`.
- [ ] Should migration be a separate command (`ars migrate`) or integrated into `validate` with a flag?
- [ ] Should pattern items use glob syntax, regex, or a custom DSL?
- [ ] What is the priority order: schema migration tooling first, or pattern support first?
- [ ] Does pattern support require a new ADR amending ADR-0005 and ADR-0006, or a new ADR that supersedes them?

---

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | v2 candidates listed in §23 |
| [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md) | Model layer and comparison engine design |
| [ADR-0003 — JSON Model Format](../adr/ADR-0003-use-json-model-format.md) | JSON-only constraint for v1 |
| [ADR-0005 — v1 Model Schema](../adr/ADR-0005-v1-model-schema.md) | Current schema definition; may need amendment if patterns are adopted |
| [ADR-0006 — Comparison Semantics](../adr/ADR-0006-comparison-semantics.md) | Current finding types; would need amendment for pattern matching |
