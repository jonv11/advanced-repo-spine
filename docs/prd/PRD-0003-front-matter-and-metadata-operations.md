# PRD-0003: Front Matter and Metadata Operations for Text-Based Files

| Field | Value |
|-------|-------|
| **Status** | Draft |
| **Owner(s)** | — |
| **Date** | 2026-03-29 |
| **Stakeholders** | ARS maintainers, repository contributors, technical leads, AI coding agent users, CI/tooling consumers |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md), [PRD-0002 — Repository Documentation Outline View](PRD-0002-repository-documentation-outline-view.md), [RFC-0006 — Model Evolution](../rfc/RFC-0006-model-evolution.md), [ADR-0003 — Use JSON as the v1 Model Format](../adr/ADR-0003-use-json-model-format.md), [ADR-0004 — v1 Read-Only](../adr/ADR-0004-v1-read-only.md) |

---

## Problem Statement

Text-based repository artifacts — Markdown documents such as ADRs, PRDs, RFCs, runbooks, and contributor guides — commonly carry structured metadata in YAML front matter blocks (a YAML section delimited by `---` at the start of a file). ARS can locate and report these files at the path level, but it cannot read, validate, or operate on the metadata they contain.

This gap means ARS cannot enforce metadata standards across a document collection. A repository may require every ADR to have a `status` field, every runbook to have an `owner` field, and every PRD to have a `date` — but ARS has no mechanism to detect missing fields, invalid values, or inconsistent formatting. Auditing front matter compliance is currently a manual task: contributors must open files individually, and no automated check signals when a repository has drifted from its own metadata conventions.

For AI coding agents, the absence of metadata queryability creates a related problem: agents cannot filter or select documents by metadata without bespoke scripting, cannot verify that a newly created document meets the repository's metadata standard, and cannot load a metadata-filtered view of a document collection as planning context.

## Background / Context

[PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md) explicitly lists front matter parsing as a v1 non-goal and names it as a v2+ candidate. [PRD-0002 — Repository Documentation Outline View](PRD-0002-repository-documentation-outline-view.md) lists front matter extraction as out of scope for the `ars outline` capability. Both were deliberate deferrals, not rejections.

ARS's structural inspection currently treats document files as opaque: it knows where they live and whether they match the model, but nothing about their content. Front matter is the structured metadata layer that sits between path-level structure and document body content. Addressing it is a natural next step after v1 establishes the structural foundation.

[RFC-0006 — Model Evolution](../rfc/RFC-0006-model-evolution.md) is an exploratory RFC examining how the ARS model format could evolve to support new rule types and schema versioning. Front matter rule set storage is a related concern; any RFC implementing this PRD must align with the model evolution direction settled by RFC-0006.

This PRD defines product requirements only. Parser architecture, rule set storage format, YAML library choices, and write safety implementation belong in a follow-on RFC.

## Goals

- Provide an `ars frontmatter` command group that can read, validate, and modify front matter in `.md` files.
- Allow users to configure front matter rule sets that define required fields, allowed values, and field type constraints.
- Validate front matter compliance for individual files or a complete file set and produce findings that are deterministic, machine-readable, and actionable.
- Support bulk front matter operations (add, update, remove, normalize) that can target a file set and are safe to run with confidence that non-front-matter content will not be changed.
- Provide metadata coverage reports: for a configured rule set, show which files meet the standard and which do not.
- Support dry-run mode for all write operations so users can preview changes before applying them.
- Produce all read, validate, and report output in both text and JSON formats from the first release of this capability.

## Non-Goals

- Editing Markdown body content beyond the minimal range required to safely insert, update, or remove a front matter block.
- Repository structure governance, path/placement enforcement, or structural diffing — covered by PRD-0005.
- Git ignore policy management — covered by PRD-0006.
- Markdown section editing, TOC generation, or link checking — covered by PRD-0004.
- Parsing TOML front matter (`+++` delimiters) in the initial release; YAML front matter (`---`) is the primary format for the first delivery.
- Integration with external schema registries or JSON Schema validation infrastructure.
- Automatic metadata inference from file names, path segments, body content, or Git history.
- Static site generation, publishing pipelines, or any output beyond repository-internal metadata management.
- Binary file metadata operations.

## Users / Stakeholders

| User / Stakeholder | Need or Impact |
|---|---|
| Repository contributors | Need to know whether their documents conform to metadata standards without manual inspection; need metadata-stamped starter files when creating new documents. |
| Technical leads and maintainers | Need to enforce metadata standards at scale and detect metadata drift across a document collection without custom scripting. |
| AI coding agents | Need a machine-readable, queryable view of document metadata to filter relevant context before planning or generating documentation. |
| ARS maintainers | Need a front matter capability that extends ARS's structural awareness without conflating document metadata management with structural governance. |
| CI/tooling consumers | Need stable, machine-readable front matter reports for automated validation pipelines. |

## Scope

### In Scope

- Parsing YAML front matter (`---`-delimited blocks at file start) from `.md` files
- Reporting front matter fields and values per file in text and JSON output formats
- Configurable front matter rule sets: required fields, optional fields, allowed value lists, and field type constraints (string, date, boolean, list)
- Per-file validation findings: missing required fields, invalid field values, unexpected fields in strict mode
- Bulk add operation: add a field with a given value to all matched files that do not already have it
- Bulk update operation: set or change a field value across all matched files
- Bulk remove operation: delete a specified field from all matched files
- Normalize operation: rewrite front matter to canonical field ordering and consistent YAML formatting without changing field values
- Dry-run mode for all write operations (add, update, remove, normalize) that previews changes without modifying files
- File selection and filtering by field presence, field absence, or field value match — usable as a standalone query and as a target selector for bulk operations
- Metadata coverage reports: for a configured rule set, per-file and aggregate presence/absence of required and optional fields
- Front matter diffing: compare the front matter in a file against a rule-set-derived expectation and report per-file differences
- Metadata templating: a named template front matter block that can be applied to new or existing files as a starting point

### Out of Scope

- Markdown body editing, section operations, TOC generation, link checking, or table normalization
- Repository structure governance, structural comparison, or structural diffing
- Git-aware workflows, `.gitignore` parsing, or tracked-vs-ignored analysis
- TOML front matter or other non-YAML front matter formats in the initial release
- External schema registry integration
- Automatic metadata inference from file content, names, or history
- Static site publishing pipelines
- Binary file metadata operations

## Functional Requirements

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-1 | ARS must parse YAML front matter (content between opening and closing `---` delimiters) from `.md` files. Files that do not begin with `---` must be treated as having no front matter. | Must | Delimiter-based detection only; content between the delimiters is parsed as YAML. |
| FR-2 | ARS must provide a command to read and report front matter fields and values for a given file or file set, supporting both text and JSON output formats. | Must | JSON output must use stable field names across releases. |
| FR-3 | ARS must support a configurable front matter rule set defining: required fields, optional fields, allowed field values (enumerated list), and field type constraints (string, date, boolean, list). | Must | Rule sets must be declarable in the ARS model or a configuration file; the storage location is an open question resolved in the follow-on RFC. |
| FR-4 | ARS must validate front matter for a file or file set against the active rule set and report per-file findings including missing required fields, invalid field values, and unexpected fields when strict mode is enabled. | Must | Validation must produce a non-zero exit code when required-field violations are present. |
| FR-5 | ARS must support a bulk add operation that adds a specified field with a specified value to all matched files that do not already have that field. | Must | Must not overwrite existing values; files already containing the field must be skipped and reported as no-ops. |
| FR-6 | ARS must support a bulk update operation that sets a specified field to a specified value across all matched files, adding the field when absent and updating it when present. | Must | — |
| FR-7 | ARS must support a bulk remove operation that deletes a specified field from all matched files; field absence in a matched file must be reported as a no-op, not an error. | Must | — |
| FR-8 | ARS must support a normalize operation that rewrites front matter to canonical field ordering and consistent YAML formatting without changing any field values. | Must | Canonical order must be configurable; defaults to alphabetical order when not specified. |
| FR-9 | ARS must guarantee that when it writes front matter changes, all file content outside the front matter block is preserved byte-for-byte. A parsing or round-trip failure must abort the write and leave the file unmodified, reporting the failure clearly. | Must | Hard safety constraint for all write operations. |
| FR-10 | ARS must support dry-run mode for all write operations (add, update, remove, normalize) that previews planned changes per file without modifying any files. | Must | Dry-run output must be human-readable in text mode and structured in JSON mode. |
| FR-11 | ARS must support filtering files by front matter field presence, absence, or value match, usable as a standalone query and as a target selector for bulk operations. | Must | Example: target only files where `status: draft` for a bulk update. |
| FR-12 | ARS must produce a metadata coverage report for a file set showing, for each configured required field, the count and list of files with and without the field. | Must | Report must support both text and JSON output formats. |
| FR-13 | ARS should support front matter diffing: compare the actual front matter in a file against a rule-set-derived expectation and report per-file differences as structured findings. | Should | Useful for CI enforcement workflows. |
| FR-14 | ARS should support metadata templates: a named front matter block that can be applied to new or existing files as a starting point without overwriting fields that are already present. | Should | Templates declared in the ARS model or configuration. |
| FR-15 | ARS should report the count and percentage of files that fully satisfy a defined metadata rule set as part of coverage reports. | Should | Provides an objective quality signal without requiring external tooling. |
| FR-16 | ARS could support TOML front matter (`+++` delimiters) as an additional input format in a future release. | Could | First release targets YAML only for broad applicability and implementation simplicity. |

## Non-Functional Requirements

| ID | Requirement | Target | Notes |
|----|-------------|--------|-------|
| NFR-1 | Deterministic output ordering | The same file set and same options produce stable text and JSON output across runs. | Required for snapshot testing and agent consumption. |
| NFR-2 | Safe write atomicity | A failed write must leave the original file in its pre-operation state; no partially written files under any failure mode. | Applies to all write operations. |
| NFR-3 | Front matter round-trip fidelity | Parsing then re-serializing an unmodified front matter block must produce byte-identical YAML output. | Verified by automated round-trip tests. |
| NFR-4 | Machine-readable output stability | JSON field names in all output contracts remain stable across ARS releases. | Required for downstream tooling and CI parsers. |

## Constraints

- ARS remains CLI-first; all front matter capabilities must be accessible through the command surface.
- Write operations (add, update, remove, normalize) must be strictly opt-in; read, validate, and report operations must remain read-only.
- Dry-run mode is required for all write operations from the first release and must be the default behavior unless an explicit apply flag is passed.
- The initial release targets `.md` files only; other text-based formats are deferred.
- Front matter rule set storage must be compatible with the model evolution direction established in RFC-0006.

## Assumptions

- YAML front matter delimited by `---` is the dominant front matter format in repositories ARS targets.
- Repositories using front matter will typically have between tens and a few hundred Markdown files; performance optimization for thousands of files is a should-have rather than a must-have for the initial release.
- Front matter rule sets will be authored and maintained by repository maintainers, not auto-discovered or generated.
- Users require a dry-run preview before any bulk write operation that touches many files.
- Text and JSON output are both required from day one because ARS serves both humans and automated consumers.
- Multiline and nested YAML field values may appear in real repositories; the RFC must specify whether these types are fully round-trippable or read-only in the initial release.

## Dependencies

| Dependency | Owner | Status | Impact if Delayed |
|------------|-------|--------|-------------------|
| Front matter rule set storage design (extension to `ars.json` model or separate configuration file) | ARS maintainers | Not started | Must be resolved in the follow-on RFC before implementation begins; affects model schema and validation tooling. |
| Model evolution direction (RFC-0006) | ARS maintainers | Exploratory | Front matter rule set storage format must align with any schema versioning decisions from RFC-0006. |
| Follow-on RFC defining command surface, YAML parsing approach, write safety strategy, and dry-run contract | ARS maintainers | Not started | Implementation must not begin without this design. |

## Edge Cases

- A `.md` file has no front matter (does not begin with `---`): treat as having empty front matter; validation must report all required fields as missing when strict mode is active.
- A file starts with `---` but has no closing `---` delimiter: treat as malformed front matter; refuse all write operations and report the parsing failure clearly without modifying the file.
- Front matter contains a multiline string or nested YAML structure: read and report as-is; write operations for scalar values must not corrupt multiline or nested values; the RFC must specify round-trip handling for non-scalar types.
- A file has a BOM or contains non-UTF-8 characters: refuse write operations and report the encoding issue; read operations must report the issue as a warning without crashing.
- The normalize operation is run on already-canonical front matter: produce a no-op result; do not write the file; report that no changes were required.
- A bulk operation targets a large file set: dry-run output must remain bounded and usable; the RFC should define pagination or summary modes for large file sets.
- Two files have front matter fields with the same name but different casing (e.g., `Status` vs `status`): treat as distinct fields; the RFC must specify whether field name matching is case-sensitive.

## Acceptance Criteria

- [ ] Given a `.md` file with YAML front matter, when the read command runs, then it outputs parsed field names and values in both text and JSON formats without error.
- [ ] Given a `.md` file without front matter (no opening `---`), when the read command runs, then it reports no front matter found without failing.
- [ ] Given a rule set with required fields, when the validate command runs against a file missing required fields, then it reports each missing field per file and exits non-zero.
- [ ] Given a field value not in the allowed-values list, when the validate command runs, then it reports the violation with file path, field name, and disallowed value.
- [ ] Given a bulk add operation targeting files lacking a specified field, when the command runs, then the field is added to matching files that lack it and files that already have the field are unchanged.
- [ ] Given a bulk update operation, when the command runs, then the specified field is set to the new value in all matched files and all non-front-matter content in each file is unchanged.
- [ ] Given a bulk remove operation on a file where the field is absent, when the command runs, then the operation completes without error and the file is unmodified.
- [ ] Given a normalize operation, when the command runs, then front matter fields are reordered to canonical order, field values are unchanged, and non-front-matter content is unchanged.
- [ ] Given dry-run mode for any write operation, when the command runs, then it outputs a preview of planned changes per file without modifying any files.
- [ ] Given a file with malformed front matter (opening `---` without a closing `---`), when any write operation is attempted, then the operation is refused and the file is unmodified.
- [ ] Given a metadata coverage report command and a configured rule set, when the command runs, then it reports per-file and aggregate coverage for all required fields in text and JSON formats.
- [ ] Given a file selection filter by field value, when the command runs, then only files matching the specified field and value are returned or targeted.
- [ ] Given repeated runs with identical inputs, then text and JSON output are stable in field ordering and result ordering.

## Success Metrics

| Metric | Baseline | Target | Measurement Method |
|--------|----------|--------|--------------------|
| Front matter validation coverage | No automated front matter validation in ARS | `ars frontmatter validate` covers required-field checking, invalid-value checking, and coverage reporting as specified in acceptance criteria | Acceptance criteria validation during follow-on RFC and implementation testing |
| Safe write reliability | No write operations beyond `ars init` in ARS | Zero cases of non-front-matter content modification across automated round-trip tests | Content-preservation tests with varied file layouts and front matter positions |
| Dry-run availability | Not applicable | Dry-run mode is supported and tested for all write subcommands from the first release | Automated test coverage of dry-run behavior for all write operations |

## Rollout Considerations

This capability introduces write operations to ARS beyond `ars init`. Write operations must be strictly opt-in and must not activate during any read, validate, or report command. Dry-run must be the default behavior for write commands unless the user explicitly passes an apply flag. The follow-on RFC should specify whether applying changes to more than a configurable threshold of files requires an additional confirmation step to prevent accidental mass modification.

## Open Questions

- [ ] Should front matter rule sets be declared inside the existing `ars.json` model as a new top-level section, or in a separate configuration file? This has implications for model schema versioning and backward compatibility with v1 models.
- [ ] What is the handling strategy for multiline YAML values and nested structures during write operations? The RFC must determine whether these types are read-only or fully round-trippable in the initial release.
- [ ] Should the normalize operation's canonical field order default to alphabetical, to the order defined in the rule set, or to a global setting in the ARS configuration?
- [ ] Should field name matching for filters and operations be case-sensitive or case-insensitive? YAML keys are case-sensitive by specification, but the RFC should clarify ARS's intended behavior.

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md) | Establishes the v1 scope and lists front matter parsing as a v1 non-goal deferred to v2+. This PRD defines that deferred capability. |
| [PRD-0002 — Repository Documentation Outline View](PRD-0002-repository-documentation-outline-view.md) | Defines `ars outline` for heading discovery; explicitly defers front matter extraction, which this PRD addresses. |
| [RFC-0006 — Model Evolution](../rfc/RFC-0006-model-evolution.md) | Exploratory RFC examining model schema versioning; relevant to where front matter rule sets should be declared and how backward compatibility should be maintained. |
| [ADR-0003 — Use JSON as the v1 Model Format](../adr/ADR-0003-use-json-model-format.md) | Records the JSON-only model format decision; relevant to front matter rule set storage and serialization format choices. |
| [ADR-0004 — v1 Read-Only Except for `init`](../adr/ADR-0004-v1-read-only.md) | Records the decision to keep v1 read-only; this PRD marks the planned post-v1 introduction of additional write operations. |
