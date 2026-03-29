# PRD-0004: Markdown Authoring, Editing, and Content Transformation

| Field | Value |
|-------|-------|
| **Status** | Draft |
| **Owner(s)** | — |
| **Date** | 2026-03-29 |
| **Stakeholders** | ARS maintainers, repository contributors, technical leads, AI coding agent users, CI/tooling consumers |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md), [PRD-0002 — Repository Documentation Outline View](PRD-0002-repository-documentation-outline-view.md), [PRD-0003 — Front Matter and Metadata Operations](PRD-0003-front-matter-and-metadata-operations.md), [RFC-0007 — `ars outline` Command Design](../rfc/RFC-0007-ars-outline-command-design.md), [ADR-0010 — ATX Headings Only in v1](../adr/ADR-0010-atx-headings-only-extraction.md), [ADR-0011 — Flat Heading List Under File Nodes](../adr/ADR-0011-flat-heading-list-under-file-nodes.md) |

---

## Problem Statement

ARS provides Markdown heading discovery through `ars outline`, but it cannot modify document content. Contributors and maintainers who need to make consistent structural updates across a Markdown document collection — inserting a new section, replacing a heading block, updating a table of contents, normalizing table formatting, detecting broken links — currently have no ARS-native mechanism for these operations. The result is that content transformation work is manual, error-prone, and inconsistently applied.

This gap is most visible in three recurring maintenance tasks. First, evolving a multi-document collection (adding a required section to all ADRs, updating a heading name in all runbooks) requires opening and editing each file individually with no automated path. Second, table of contents blocks go stale as headings change, and there is no way to regenerate them without manual editing. Third, broken internal links — references to anchors or relative file paths that no longer exist — accumulate silently with no detection tool.

For AI coding agents, the absence of structure-aware editing creates a correctness risk: a naive text replacement that does not understand document structure can corrupt content outside the intended target region. ARS is well-positioned to provide safe, heading-scoped operations that bound the edit to the intended section and preserve everything else.

## Background / Context

[PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md) lists Markdown editing, automatic TOC generation, and automatic index generation explicitly as v1 non-goals and names all three as v2+ candidates. [PRD-0002 — Repository Documentation Outline View](PRD-0002-repository-documentation-outline-view.md) adds read-only heading discovery via `ars outline`, establishing the heading extraction foundation that structure-aware write operations depend on.

This PRD defines the complementary write/transform path: section-level operations that use the same heading structure `ars outline` already extracts, applied to editing, TOC management, link integrity checking, table normalization, and content linting. The heading extraction decisions in [ADR-0010 — ATX Headings Only in v1](../adr/ADR-0010-atx-headings-only-extraction.md) and [ADR-0011 — Flat Heading List Under File Nodes](../adr/ADR-0011-flat-heading-list-under-file-nodes.md) directly inform section identification behavior for this PRD.

## Goals

- Provide section-level operations (extract, insert, replace) that identify target sections by heading and apply changes without modifying content outside the targeted region.
- Provide TOC generation and update commands that manage a table of contents block in a Markdown document.
- Provide a link integrity check command that detects broken internal links (and optionally external links) in a Markdown file or file set.
- Provide a table normalization command that reformats GFM-style tables to consistent column alignment without changing cell values.
- Provide a Markdown content lint command that reports structural violations against a configurable rule set, with non-zero exit code for CI integration.
- Guarantee that all write operations preserve content outside the targeted region byte-for-byte.
- Support dry-run mode for all write operations from the first release.
- Produce lint and link-check reports in both text and JSON output formats.

## Non-Goals

- Front matter reading, writing, or validation — covered by PRD-0003.
- Repository structure governance, path/placement enforcement, or structural diffing — covered by PRD-0005.
- Git ignore policy management — covered by PRD-0006.
- Rendering Markdown to HTML, PDF, or any publishable format.
- Full WYSIWYG editing or rich text editor UI.
- Converting between Markdown dialects (CommonMark, GFM, MDX, AsciiDoc) as a general migration workflow.
- Generating documentation from source code comments or other non-Markdown input sources.
- Parsing or transforming `.mdx`, AsciiDoc, reStructuredText, or other non-`.md` file types in the initial release.

## Users / Stakeholders

| User / Stakeholder | Need or Impact |
|---|---|
| Repository contributors | Need to make consistent structural updates across Markdown documents without manual editing of each file. |
| Technical leads and maintainers | Need to enforce Markdown structural conventions, detect content quality issues, and keep TOC blocks current at scale. |
| AI coding agents | Need heading-scoped edit operations that modify only the intended section and preserve surrounding content, reducing the risk of structural corruption during automated editing. |
| ARS maintainers | Need a Markdown editing capability that extends the heading discovery foundation from PRD-0002 without redesigning its semantics. |
| CI/tooling consumers | Need machine-readable lint and link-check reports for automated quality gates. |

## Scope

### In Scope

- Section extraction: output the content of a section identified by heading (text, level, or heading path) from a Markdown file
- Section insertion: insert content before or after a specified section in a Markdown file
- Section replacement: replace the full content of a named section (including all sub-headings and their content) with new content
- TOC generation: generate a table of contents from a document's heading structure and insert it at a configurable location
- TOC update: update an existing ARS-managed TOC block in a file when headings have changed
- Internal link checking: detect broken anchor references and broken relative file-path references in a file or file set
- Table normalization: reformat GFM pipe-delimited tables to consistent column alignment and whitespace without changing cell content
- Markdown content linting: configurable rule set covering heading level skips, blank line requirements, list marker consistency, and trailing whitespace
- Context-aware search and replace: find and replace text within the scope of a specified section heading rather than across the full file
- Batch mode: apply any of the above operations across a file set matched by path pattern
- Dry-run mode for all write operations
- Text and JSON output formats for lint and link-check reports

### Out of Scope

- Front matter reading, writing, or validation
- Repository structure governance or structural comparison
- Git-aware workflows or ignore policy
- Markdown-to-HTML or other rendering formats
- WYSIWYG or IDE editing UI
- MDX, AsciiDoc, reStructuredText, or other non-`.md` file format support in the initial release
- Automatic content generation from non-structural inputs (e.g., documentation extracted from source code)
- Section movement (reordering sections within a document) in the initial release

## Functional Requirements

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-1 | ARS must support section extraction: given a Markdown file and a heading identifier, output the full section content including the heading line and all sub-heading content. | Must | Section identification must be deterministic; the RFC must specify how duplicate headings are disambiguated. |
| FR-2 | ARS must support section insertion: given a Markdown file, a heading identifier, and an insertion position (before or after the identified section), insert provided content at the specified location. | Must | All content outside the insertion point must be preserved byte-for-byte. |
| FR-3 | ARS must support section replacement: given a Markdown file and a heading identifier, replace the section content (including all sub-headings and their content) with provided content. | Must | Content outside the replaced section must be preserved byte-for-byte. |
| FR-4 | ARS must support TOC generation: given a Markdown file, generate a table of contents from the current heading structure and insert it at a configurable position in the document. | Must | TOC format details (anchor style, indentation) belong in the RFC; this requirement establishes the product capability. |
| FR-5 | ARS must support TOC update: given a Markdown file with an ARS-managed TOC block, update the TOC to reflect the current heading structure without requiring the user to regenerate manually. | Must | Manually authored TOC blocks without an ARS marker must be left unchanged; the command must report that they were not recognized. |
| FR-6 | ARS must support internal link checking: given a Markdown file or file set, detect and report anchor references and relative file-path references that are broken (the target does not exist). | Must | Reports must include the source file path, the broken reference text, and the expected target. |
| FR-7 | ARS must support table normalization: given a Markdown file or file set, reformat GFM pipe-delimited tables to consistent column alignment and whitespace without changing cell content. | Must | Tables with inconsistent column counts must be reported rather than silently modified; non-table content must be preserved. |
| FR-8 | ARS must support Markdown content linting with a configurable rule set, reporting structural violations with a non-zero exit code when violations are present. | Must | Minimum initial rules: heading level skips (e.g., H1 immediately followed by H3), missing blank lines between sections, inconsistent list markers within a list, trailing whitespace. The RFC defines the full lint rule vocabulary and rule ID format. |
| FR-9 | ARS must guarantee that all write operations preserve all content outside the targeted region (section, TOC block, or table) byte-for-byte. A parsing or round-trip failure must abort the write without modifying the file and must report the failure clearly. | Must | Hard safety constraint for all write operations. |
| FR-10 | ARS must support dry-run mode for all write operations, outputting a preview of planned changes per file without modifying any files. | Must | Dry-run output must be human-readable in text mode and structured in JSON mode. |
| FR-11 | ARS must support context-aware search and replace: find and replace text within the scope of a specified section heading rather than globally across the full file. | Must | Global unscoped search-and-replace is not in scope for this PRD. |
| FR-12 | ARS should support batch mode: apply any section, TOC, table normalization, lint, or link-check operation across a file set matched by path pattern. | Should | Batch targeting should integrate with front matter field filtering from PRD-0003 as a should-level integration. |
| FR-13 | ARS should support external link checking as an opt-in mode, verifying that external URLs referenced in a Markdown file return valid HTTP responses. | Should | Network operations must be opt-in and must not activate by default. |
| FR-14 | ARS could support section movement: reorder two or more sections within a document given source and destination heading identifiers. | Could | Deferred from the initial release due to implementation complexity; defined here to prevent ambiguity with future work. |
| FR-15 | ARS could support setext-to-ATX heading normalization as a write operation, converting underline-style headings to `#`-prefix style throughout a file. | Could | Aligns with the ATX-only direction in ADR-0010 but extends it to a write operation. |

## Non-Functional Requirements

| ID | Requirement | Target | Notes |
|----|-------------|--------|-------|
| NFR-1 | Deterministic output | The same file and same operation produce identical output across runs. | Required for snapshot testing and agent consumption. |
| NFR-2 | Safe write atomicity | A failed write must leave the file in its pre-operation state; no partially written files under any failure mode. | Applies to all write operations. |
| NFR-3 | Content preservation | Non-targeted content is preserved byte-for-byte on every write operation. | Verified by automated round-trip tests covering varied document layouts. |
| NFR-4 | Lint rule ID stability | Lint violations reference stable rule identifiers across ARS releases. | Required for CI integrations that suppress or gate on specific rule IDs. |

## Constraints

- ARS remains CLI-first; all Markdown editing capabilities must be accessible through the command surface.
- Write operations are strictly opt-in; read, extract, lint, and link-check operations must remain read-only.
- Dry-run mode is required for all write operations from the first release.
- ATX-style headings (`#`-prefix) are the primary target format for heading identification, consistent with ADR-0010.
- Setext-style headings (underline `===` and `---`) may appear in input files; the RFC must specify whether they are supported for section targeting or whether the user is prompted to normalize to ATX first.
- The initial release targets `.md` files; other document formats are out of scope.

## Assumptions

- ATX-style headings are dominant in repositories ARS targets; this is consistent with ADR-0010's extraction scope.
- GFM pipe-delimited tables are the target format for table normalization.
- Internal link integrity (anchors and relative file paths) provides the primary value for link checking; external link checking is an important secondary capability.
- Users require a dry-run preview before any write operation that changes Markdown body content.
- The heading extraction foundation from PRD-0002 (ADR-0010, ADR-0011, RFC-0007) is stable enough to build section-level write operations on top of.

## Dependencies

| Dependency | Owner | Status | Impact if Delayed |
|------------|-------|--------|-------------------|
| `ars outline` heading extraction capability (PRD-0002) | ARS maintainers | Approved; RFC-0007 in progress | Section identification depends on reliable heading extraction; ADR-0010 and ADR-0011 define the heading model used for section targeting. |
| Follow-on RFC defining command surface, section identification semantics, TOC marker format, lint rule vocabulary, safe write strategy, and link-check contract | ARS maintainers | Not started | Implementation must not begin without this design. |
| PRD-0003 front matter operations | ARS maintainers | Draft | Batch file targeting may optionally integrate with front matter field filters; a should-level dependency. |

## Edge Cases

- A heading text appears more than once in the same document: the RFC must specify whether section operations target the first match, require a positional index, or require a full heading path for disambiguation.
- A section contains sub-headings at multiple levels: section extraction and replacement must include all sub-content unless explicitly scoped to a specific heading depth.
- A file uses setext-style headings for a targeted section: the RFC must specify whether these are supported for targeting, silently treated as ATX equivalents, or treated as unsupported with a clear error.
- TOC update is run on a file with a manually authored TOC that has no ARS-managed marker: the command must leave the TOC unchanged and report that it was not recognized as ARS-managed.
- A relative link target file exists but has been renamed: link checking must report the broken reference using the original target string from the source file.
- A GFM table has rows with inconsistent column counts: normalization must report the structural inconsistency and decline to modify the affected table; other tables in the same file may still be normalized.
- A write operation fails midway through a file (e.g., disk full): the original file must not be left in a partially written state; the RFC must specify the atomic write protocol.
- Dry-run on a large file set produces large output: the RFC should define summary modes or output limits for large batch operations.

## Acceptance Criteria

- [ ] Given a Markdown file and a valid heading identifier, when the section extract command runs, then it outputs the heading line and all sub-heading content for that section.
- [ ] Given a section insertion command with a valid heading target, when the command runs, then content is inserted at the specified position and all existing content outside the insertion point is preserved identically.
- [ ] Given a section replacement command, when the command runs, then the specified section is replaced with the new content and all content outside the section is preserved identically.
- [ ] Given a Markdown file, when the TOC generate command runs, then a table of contents reflecting current headings is inserted at the configured position without changing any other content.
- [ ] Given a Markdown file with an ARS-managed TOC block and modified headings, when the TOC update command runs, then the TOC reflects the current heading structure.
- [ ] Given a Markdown file with broken internal anchor references or broken relative file-path links, when the link check command runs, then broken references are reported with source file path and the broken reference text.
- [ ] Given a Markdown file with GFM tables, when the table normalize command runs, then tables have consistent column formatting and all non-table content is unchanged.
- [ ] Given a lint rule set and a Markdown file with violations, when the lint command runs, then each violation is reported with rule ID and file path, and the exit code is non-zero.
- [ ] Given dry-run mode for any write operation, when the command runs, then it outputs a preview of all planned changes without modifying any files.
- [ ] Given a write operation where parsing fails or round-trip safety cannot be guaranteed, then the file is not modified and the failure is reported clearly.
- [ ] Given a Markdown file with a search term scoped to a named section, when the context-aware search and replace command runs, then occurrences within the specified section are replaced and all text outside that section is unchanged.
- [ ] Given repeated runs with identical inputs, then lint and link-check report output are stable in result ordering.

## Success Metrics

| Metric | Baseline | Target | Measurement Method |
|--------|----------|--------|--------------------|
| Section-level editing capability | ARS provides no file editing beyond `ars init` | Section extract, insert, replace, and TOC operations are available and pass all acceptance criteria | Acceptance criteria validation during follow-on RFC and implementation testing |
| Safe write reliability | Not applicable | Zero cases of non-targeted content modification across automated round-trip tests | Content-preservation tests covering varied document layouts and section types |
| Link integrity detection | No link checking in ARS | Broken internal links are detectable with one command and reported in text and JSON formats | Fixture-based link-check tests with known-broken references |

## Rollout Considerations

This capability introduces Markdown body write operations alongside the front matter write operations described in PRD-0003. Write operations must be clearly separated from read and report operations in the command surface. Dry-run must be the default behavior for write commands unless an explicit apply flag is passed. The RFC should define an atomic write protocol (e.g., write to a temporary file and atomically replace on success) to satisfy the content preservation constraint under failure conditions.

## Open Questions

- [ ] How should duplicate headings within the same document be disambiguated for section targeting? The RFC must specify a deterministic selection rule (e.g., first match, positional index, or full heading path).
- [ ] Should setext-style headings in the input be supported for section targeting, or treated as an error that prompts the user to normalize to ATX first?
- [ ] What marker format should ARS use to identify ARS-managed TOC blocks, so they can be updated reliably without risk of corrupting manually authored TOC content?

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md) | Lists Markdown editing and TOC generation as v1 non-goals and v2+ candidates. This PRD defines those capabilities. |
| [PRD-0002 — Repository Documentation Outline View](PRD-0002-repository-documentation-outline-view.md) | Defines `ars outline` for heading discovery (read path); this PRD defines the complementary write/transform path. |
| [PRD-0003 — Front Matter and Metadata Operations for Text-Based Files](PRD-0003-front-matter-and-metadata-operations.md) | Defines front matter write operations; batch file targeting in this PRD may integrate with front matter field filters from PRD-0003. |
| [RFC-0007 — `ars outline` Command Design](../rfc/RFC-0007-ars-outline-command-design.md) | Specifies heading extraction algorithm and output contract; the follow-on RFC for this PRD should extend or reference the heading model defined there. |
| [ADR-0010 — ATX Headings Only in v1](../adr/ADR-0010-atx-headings-only-extraction.md) | Records the decision to scope heading extraction to ATX-style headings; informs section identification behavior for write operations in this PRD. |
| [ADR-0011 — Flat Heading List Under File Nodes](../adr/ADR-0011-flat-heading-list-under-file-nodes.md) | Records the heading list output contract; informs how section depth and heading level are represented in section operations. |
