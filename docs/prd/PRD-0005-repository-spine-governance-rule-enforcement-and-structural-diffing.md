# PRD-0005: Repository Spine Governance, Rule Enforcement, and Structural Diffing

| Field | Value |
|-------|-------|
| **Status** | Draft |
| **Owner(s)** | — |
| **Date** | 2026-03-29 |
| **Stakeholders** | ARS maintainers, repository contributors, senior engineers, platform and architecture owners, AI coding agent users, CI/tooling consumers |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md), [PRD-0002 — Repository Documentation Outline View](PRD-0002-repository-documentation-outline-view.md), [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md), [RFC-0006 — Model Evolution](../rfc/RFC-0006-model-evolution.md), [ADR-0005 — v1 Model Schema](../adr/ADR-0005-v1-model-schema.md), [ADR-0006 — Comparison Semantics](../adr/ADR-0006-comparison-semantics.md) |

---

## Problem Statement

ARS v1 compares a repository against a model that specifies exact expected paths and produces findings of type Missing, Present, OptionalMissing, Unmatched, or Misplaced. This model is effective for repositories with a small, stable set of well-known paths, but it fails to express two patterns that are common in real repositories.

First, document collections that follow naming conventions — ADRs named `ADR-NNNN-*.md`, RFCs named `RFC-NNNN-*.md`, sprint retros named `retro-YYYY-MM.md` — cannot be represented as exact paths because the set of files grows over time. The only valid representation is a pattern, but v1 supports only exact-path matching. This forces model authors to either enumerate every file explicitly (making the model high-maintenance and fragile as the collection grows) or leave the document collection unmodeled (losing enforcement value entirely). A repository using ARS for ADR governance today must update `ars.json` every time a new ADR is added, or accept that new ADRs are always Unmatched.

Second, ARS v1 has no mechanism to detect structural change over time. A repository that passes comparison today and also passes it six months from now might have undergone significant structural reorganization in between — documentation moved, directories consolidated, required artifacts removed and re-added. The model may have been updated in parallel to reflect each change, so every point-in-time comparison passes, but the structural drift itself is invisible. Without a structural baseline and drift detection mechanism, ARS cannot answer "how has this repository's structure changed since its last review?"

Both gaps reduce ARS's value as a governance and enforcement tool: models that cannot express patterns stay low-coverage, and comparisons without temporal awareness miss drift entirely.

## Background / Context

[PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md) establishes the foundational structural model (JSON, exact paths, required/optional items) and the comparison engine. Section 23 of PRD-0001 explicitly lists richer pattern rules, naming convention enforcement, and model evolution as v2+ backlog items.

[RFC-0006 — Model Evolution](../rfc/RFC-0006-model-evolution.md) is an exploratory RFC that examines the design space for model schema versioning and glob/pattern rule support. It represents unresolved design thinking rather than accepted decisions. The follow-on RFC for this PRD must align with whatever direction RFC-0006 settles.

[ADR-0005 — v1 Model Schema](../adr/ADR-0005-v1-model-schema.md) defines the current model shape. [ADR-0006 — Comparison Semantics](../adr/ADR-0006-comparison-semantics.md) defines the current finding types and the conservative misplacement detection policy; new finding types introduced by this PRD must be defined consistently with that policy.

[PRD-0002 — Repository Documentation Outline View](PRD-0002-repository-documentation-outline-view.md) adds heading discovery via `ars outline`. That capability surfaces the internal heading structure of Markdown files, which is relevant here: a structural rule might require not only that a file exist at a given path but also that it contain required sections. This PRD includes requirements for incorporating heading-level constraints into structural rules as an optional, additive layer.

This PRD is about product requirements. Architecture, glob syntax design, snapshot storage format, and severity model implementation belong in follow-on RFCs.

## Goals

- Extend the ARS structural model to support pattern-based path rules (glob/wildcard) so document collections following naming conventions can be modeled and validated without enumerating every file explicitly.
- Extend the ARS structural model to support naming convention rules that validate path segment format for files and directories within a defined scope.
- Introduce a structural snapshot capability: a machine-readable export of the observed repository structure at a given point in time that can serve as a reference baseline.
- Introduce structural drift detection: compare the current repository structure (or a snapshot) against a saved baseline snapshot and report additions, removals, and movements.
- Extend structural findings with severity levels (error, warning, info) and configurable exit code behavior per severity.
- Ensure every structural finding includes diagnostic context identifying the matched rule and explaining the finding in human-readable terms.
- Support an optional heading-level requirement in structural rules for `.md` files, enabling structural rules to specify expected section structure.
- Extend the ARS model with a schema version field to enable safe forward-compatible evolution.

## Non-Goals

- Editing file content of any kind — covered by PRD-0004.
- Front matter reading, writing, or validation — covered by PRD-0003.
- Git ignore policy, `.gitignore` analysis, or tracked-vs-ignored filtering — covered by PRD-0006.
- Model storage implementation details, parser architecture, or schema migration tooling.
- Remote repository analysis without a local working directory.
- Multi-repository orchestration.
- Code-level semantic analysis (dependency graphs, import tracing, language-specific conventions).
- Any operation that mutates repository content; all governance operations must remain read-only.

## Users / Stakeholders

| User / Stakeholder | Need or Impact |
|---|---|
| Repository contributors | Need clear, actionable governance findings when structure deviates from intent, including naming convention violations that would be invisible in v1. |
| Senior engineers and architecture owners | Need to express structural conventions as patterns rather than path lists, and need drift detection to observe how repository structure evolves across reviews. |
| Technical leads and platform owners | Need severity-aware enforcement: some rules should block CI; others should produce informational warnings. |
| AI coding agents | Need precise, machine-readable structural findings with diagnostic context to understand what structural violation occurred and how to resolve it. |
| ARS maintainers | Need a model evolution path that extends v1's structural foundation without breaking backward compatibility with existing models. |
| CI/tooling consumers | Need stable, machine-readable comparison output with severity classification and rule attribution. |

## Scope

### In Scope

- Pattern-based path rules in the ARS model: glob/wildcard expressions that match multiple files by naming convention without explicit enumeration
- Naming convention rules: configurable requirements on path segment format for files and directories within a defined scope
- A new finding type for naming convention violations, distinct from Missing, Unmatched, and Misplaced
- Model schema versioning: a schema version field in the ARS model with defined forward-compatibility behavior
- Structural snapshot export: a command or mode that saves the observed repository structure (files, directories, metadata) as a timestamped, machine-readable baseline
- Structural drift comparison: compare two structural snapshots and report additions, removals, and movements between the two states
- Severity levels for findings: error, warning, and info, with configurable exit code behavior
- Diagnostic context in findings: each finding must identify the matched rule, the expected pattern, and provide a human-readable explanation
- Cardinality constraints in pattern-based rules: minimum and/or maximum count of files expected to match a pattern within a scope
- Optional heading-level requirements in structural rules: a rule may specify that files matching a pattern must satisfy a heading constraint

### Out of Scope

- File content editing, section operations, TOC management, or table normalization
- Front matter reading, validation, or writing
- Git ignore policy, `.gitignore` analysis, or tracked-vs-ignored integration
- Remote repository or multi-repository analysis
- Code-level semantic analysis or language-specific tooling
- Model storage or schema migration implementation details

## Functional Requirements

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-1 | The ARS model must support pattern-based path rules: a rule may specify a glob/wildcard expression that matches files by naming convention rather than exact path. | Must | The specific glob syntax (e.g., `*`, `**`, `?`, character classes) must be defined in the follow-on RFC based on RFC-0006's model evolution direction. |
| FR-2 | Pattern-based rules must be backward compatible with v1 exact-path rules: an existing v1 model must continue to load and produce correct comparison results in all future ARS versions. | Must | Backward compatibility is a hard constraint; schema versioning must enforce it. |
| FR-3 | The ARS model must support naming convention rules: a rule that specifies a required name format for files or directories within a defined scope and reports violations when matched paths do not conform. | Must | The format expression type (regex, glob, named pattern) must be defined in the RFC. |
| FR-4 | ARS must report naming convention violations as a finding type separate from Missing, Unmatched, and Misplaced, with a clear message identifying the non-conforming path and the expected format. | Must | Consistent with the conservative finding-type design in ADR-0006. |
| FR-5 | The ARS model must carry a schema version field, and ARS must report a clear, actionable error when it encounters an unknown or unsupported schema version rather than silently misinterpreting the model. | Must | Schema-version-less models (v1 models) must be treated as schema version `1.0` and must load successfully. |
| FR-6 | ARS must provide a structural snapshot capability: a command or mode that exports the observed repository structure at the current point in time as a timestamped, machine-readable document. | Must | Snapshots serve as baselines for drift detection. JSON is the required format. |
| FR-7 | ARS must support structural drift comparison: given two structural snapshots, produce a diff report identifying additions (new paths), removals (deleted paths), and movements (same name, different path) between the two states. | Must | Drift comparison must produce both text and JSON output. |
| FR-8 | ARS must support severity levels for structural findings: error, warning, and info. Rules in the model must be associable with a severity level, and exit code behavior must respect severity (error always exits non-zero; warning and info behavior is configurable). | Must | Allows CI enforcement to gate only on error-severity findings without treating every informational signal as a blocker. |
| FR-9 | ARS must include diagnostic context in every structural finding: the finding must identify the rule that generated it, the expected pattern or path, and a human-readable explanation of the violation. | Must | Required for agent-driven resolution and human understanding without needing to read the model. |
| FR-10 | ARS should support cardinality constraints in pattern-based rules: a rule may specify a minimum count and/or a maximum count of files expected to match the pattern within its scope. | Should | Example: at least one ADR must match `docs/adr/ADR-???-*.md`. Zero-match on a required pattern must produce a finding. |
| FR-11 | ARS should support optional heading-level requirements in structural rules: a rule may specify that files matching a given pattern must satisfy a heading constraint (e.g., must have at least one H1, must contain a section with a specified heading text). | Should | Requires integration with heading extraction from PRD-0002; applies to `.md` files only. Heading extraction failures must produce bounded findings without aborting the full run. |
| FR-12 | ARS should extend the `ars validate` command to check pattern syntax, naming convention rule format, cardinality values, and schema version compatibility, producing clear errors for any structural rule problems. | Should | Extends the v1 validate capability to cover the new rule types. |
| FR-13 | ARS could support policy profiles: named collections of rules that can be selectively activated for specific workflows (e.g., `ci-strict` activates only error-severity rules, `developer-guidance` activates all rules). | Could | Useful for teams with multiple enforcement contexts; implementation complexity makes this a could for the initial release. |
| FR-14 | ARS could support baseline pinning: designate a snapshot as the reference baseline so that subsequent drift comparisons default to that baseline without requiring an explicit snapshot path argument. | Could | Simplifies the drift detection workflow for repositories that use ARS in CI. |

## Non-Functional Requirements

| ID | Requirement | Target | Notes |
|----|-------------|--------|-------|
| NFR-1 | Deterministic output | The same repository state, same model, and same options produce stable text and JSON output across runs. | Required for CI integration and snapshot-based regression testing. |
| NFR-2 | Backward compatibility | v1 models must load and produce correct results in all future ARS versions. | Hard constraint; schema versioning must enforce it. |
| NFR-3 | Machine-readable output stability | JSON finding fields, snapshot fields, and drift report fields remain stable across ARS releases. | Required for downstream CI tooling and parsers. |
| NFR-4 | Cross-platform pattern consistency | Pattern-based rules produce the same match results on Windows, Linux, and macOS. | Path separator normalization must apply before pattern matching. |

## Constraints

- ARS remains CLI-first; all governance and structural comparison capabilities must be accessible through the command surface.
- Governance operations (compare, snapshot, drift) must remain read-only with respect to repository content.
- Backward compatibility with v1 models is a hard constraint; the schema version field must gate any breaking format change.
- Pattern-based rule syntax must be deterministic and platform-independent; the RFC must explicitly address path separator normalization before pattern evaluation.
- Heading-level requirements in structural rules depend on the heading extraction established in PRD-0002; the RFC must specify how heading extraction failures (malformed or headingless files) are handled in the context of structural rule evaluation.

## Assumptions

- Glob/pattern support is the highest-value model extension for repositories with growing document collections; exact-path rules remain valid and supported.
- Structural drift is a genuine operational concern for teams that use ARS in long-running CI; snapshot export and diff comparison are needed to surface it.
- Most severity classification will use two levels in practice (error and warning); the info level is included for completeness and future tooling integration.
- Heading-level requirements in structural rules will be used primarily to enforce documentation section standards (e.g., requiring a `## Status` section in ADRs); full heading tree matching at depth is not required for the initial implementation.
- RFC-0006's model evolution direction will be settled before or concurrently with the RFC for this PRD; pattern syntax and schema version handling cannot be finalized before RFC-0006 reaches a conclusion.

## Dependencies

| Dependency | Owner | Status | Impact if Delayed |
|------------|-------|--------|-------------------|
| RFC-0006 model evolution direction (schema versioning, glob syntax) | ARS maintainers | Exploratory | Pattern rule syntax and schema version handling must align with RFC-0006 decisions; delay blocks the RFC for this PRD. |
| `ars outline` heading extraction capability (PRD-0002) | ARS maintainers | Approved; RFC-0007 in progress | Heading-level structural requirements depend on stable heading extraction; a should-level dependency. |
| Follow-on RFC defining pattern syntax, naming convention rule format, snapshot shape, severity model, drift report contract, and cardinality semantics | ARS maintainers | Not started | Implementation must not begin without this design. |

## Edge Cases

- A pattern-based rule matches zero files in the current repository: must be reported as a distinct finding (zero matches for a required pattern) rather than silently passing when a minimum cardinality of one or more is expected.
- A pattern matches more files than a cardinality maximum: all matches must be reported in findings, not only the excess count.
- A v1 model (no schema version field) is loaded by an updated ARS: must be treated as schema version `1.0` and load successfully; must not be rejected as having an unknown version.
- A model declares a schema version higher than the current ARS version: ARS must report a clear error and suggest upgrading ARS, rather than attempting to process the model with an incomplete understanding.
- Two snapshot files being compared were produced by different ARS versions with incompatible snapshot shapes: drift comparison must detect and report the version incompatibility rather than producing incorrect diff output.
- A heading-level structural rule applies to a file with malformed Markdown: the heading extraction failure must produce a bounded finding for that file without aborting the full comparison run.
- A naming convention rule is expressed as a regex that could cause catastrophic backtracking: the RFC must specify whether pattern complexity is validated at model load time or deferred to match time.
- A structural drift comparison is run against a repository with uncommitted changes: the RFC must clarify what "current state" means in this context (filesystem state vs. Git-tracked state).

## Acceptance Criteria

- [ ] Given an ARS model with a pattern-based rule and a repository where matching files exist, when `ars compare` runs, then matching files are validated against the rule and findings are produced for non-conforming matches.
- [ ] Given a v1 model (no schema version field), when loaded by an updated ARS version, then it is treated as schema version `1.0` and produces correct comparison results without error.
- [ ] Given a model with an unrecognized schema version, when ARS loads it, then ARS exits with a clear error identifying the unsupported version and suggesting an upgrade.
- [ ] Given a naming convention rule and files in scope that do not match the required name format, when `ars compare` runs, then naming convention violations are reported as a distinct finding type with the non-conforming path and expected format.
- [ ] Given a model with some rules at `error` severity and others at `warning` severity, when `ars compare` runs with only warning-level violations present, then the exit code is zero (or configurable-zero), and when error-level violations are present, then the exit code is non-zero.
- [ ] Given a structural snapshot export command, when the command runs, then it produces a timestamped, machine-readable JSON document representing the observed repository structure.
- [ ] Given two structural snapshots, when the drift comparison runs, then it reports additions, removals, and movements between the two states in both text and JSON formats.
- [ ] Given any structural finding, then both its text and JSON output include the rule ID that generated it and a human-readable explanation of the violation.
- [ ] Given a pattern-based rule with a cardinality minimum that is not met, when `ars compare` runs, then a finding is reported for the insufficient match count.
- [ ] Given a structural rule with a heading requirement and a matching file that lacks the required heading, when `ars compare` runs, then a finding is reported for the heading violation without aborting the full run.
- [ ] Given unchanged repository contents and identical model and options, when `ars compare` is run repeatedly, then text and JSON output are stable in ordering and content.

## Success Metrics

| Metric | Baseline | Target | Measurement Method |
|--------|----------|--------|--------------------|
| Pattern rule coverage | v1 supports exact-path matching only | Document collections with naming conventions (ADRs, RFCs, runbooks) can be modeled and validated with pattern rules without per-file enumeration | Acceptance criteria validation; fixture repositories with growing document collections |
| Structural drift visibility | No temporal comparison exists in ARS | Structural drift between two repository states is detectable and reportable with a single command | Acceptance criteria validation; fixture-based drift comparison tests |
| Severity-aware enforcement | All v1 findings have undifferentiated severity | Findings are classifiable as error, warning, or info; CI gates can target only error-severity findings | Acceptance criteria validation; exit code behavior tests |

## Rollout Considerations

Pattern-based rules and naming convention rules are additive model features and must not break existing v1 models. The schema version field must be introduced in a backward-compatible way, treating schema-version-less models as v1. Structural snapshot and drift comparison are new commands that do not alter existing command behavior.

Severity levels change exit code behavior and must be documented clearly so that existing CI integrations can opt into severity-aware gating without unintended breakage. The RFC should specify the default severity for rules that do not declare one explicitly, to ensure v1-style models produce consistent behavior after upgrade.

## Open Questions

- [ ] What glob syntax should ARS support for pattern-based rules? The RFC must evaluate a minimal, well-defined subset (e.g., `*`, `**`, `?`, character classes) against common repository naming conventions and settle on platform-consistent semantics, aligned with RFC-0006.
- [ ] Should naming convention rules use regex patterns, glob expressions, or a named set of built-in conventions (e.g., `ADR-sequential`, `ISO8601-date`)? The RFC must weigh expressiveness against authoring ease and tooling complexity.
- [ ] How should structural drift comparison handle renames: as a detected move (same name, different path) or conservatively as a separate removal and addition? The RFC must define whether name-similarity heuristics are used, consistent with the conservative policy in ADR-0006.
- [ ] When heading-level requirements are part of a structural rule, how should failures from malformed or headingless `.md` files be reported — as a structural finding, a distinct heading finding, or both?

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md) | Defines the v1 structural model, comparison semantics, and command surface that this PRD extends. Section 23 of PRD-0001 explicitly identifies richer pattern rules and naming conventions as v2+ backlog. |
| [PRD-0002 — Repository Documentation Outline View](PRD-0002-repository-documentation-outline-view.md) | Defines heading extraction via `ars outline`; heading-level structural requirements in this PRD depend on that capability. |
| [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) | Defines the existing CLI architecture and layer separation; new governance commands must fit within this architecture. |
| [RFC-0006 — Model Evolution](../rfc/RFC-0006-model-evolution.md) | Exploratory RFC examining model schema versioning and glob/pattern support; the follow-on RFC for this PRD must align with RFC-0006's settled direction. |
| [ADR-0005 — v1 Model Schema](../adr/ADR-0005-v1-model-schema.md) | Records the current model shape; pattern rules, naming convention rules, and schema versioning extend this baseline. |
| [ADR-0006 — Comparison Semantics](../adr/ADR-0006-comparison-semantics.md) | Records the v1 finding types and conservative misplacement policy; new finding types from this PRD must be defined with the same conservative discipline. |
