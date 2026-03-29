# PRD-0002: Repository Documentation Outline View

| Field | Value |
|-------|-------|
| **Status** | Approved |
| **Owner(s)** | — |
| **Date** | 2026-03-29 |
| **Stakeholders** | ARS maintainers, repository contributors, AI coding agent users, technical leads, CI/tooling consumers |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md), [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md), [RFC-0003 — CLI Ergonomics and Discoverability Baseline](../rfc/RFC-0003-cli-ergonomics.md) |

---

## Problem Statement

ARS currently helps users understand repository structure at the path level, but it does not provide a single CLI view that also exposes the internal heading structure of Markdown documentation. A contributor or AI coding agent can identify where `README.md`, `docs/`, or an ADR directory live, but cannot learn the section layout inside those documents without opening each file separately.

This gap creates friction in the exact discovery workflows ARS is meant to support:

- contributors onboarding to an unfamiliar repository need both path-level orientation and document-level orientation
- maintainers reviewing documentation quality cannot quickly see which areas have rich structure versus thin or headingless documentation
- AI coding agents need a compact, deterministic context view that shows both where documentation lives and how it is organized internally

Without a combined outline view, repository discovery remains partially manual and ARS stops short of representing documentation structure as a first-class, inspectable part of repository understanding.

## Background / Context

[PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md) defines ARS as a CLI that explains, validates, and guides repository structure for humans and AI agents. The current accepted CLI surface, described in [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md), focuses on model initialization, validation, comparison, reporting, suggestion, and export. That surface is intentionally path-centric.

This proposed post-v1 extension adds a complementary discovery capability rather than altering the existing comparison workflow. The intent is to help users answer a broader discovery question: not only "what files and directories exist here?" but also "how is the documentation inside those files structured?"

A dedicated documentation outline view aligns with ARS's mission in three ways:

- it improves repo discovery for humans
- it improves context loading for AI agents before planning or implementation work
- it keeps the product CLI-first, deterministic, and automation-friendly

This PRD deliberately stops at product requirements. Parser details, rendering conventions, and internal architecture should be addressed in a follow-on RFC if this work is accepted.

## Goals

- Provide a dedicated `ars outline` command that outputs a combined repository structure and Markdown heading outline view.
- Support both human-readable text output and machine-readable JSON output from the first release of this capability.
- Allow users to run the command against the repository root or a selected subtree.
- Allow users to limit filesystem depth and Markdown heading depth for readability and scale.
- Preserve Markdown heading order and heading levels so document structure remains understandable.
- Keep command behavior deterministic and reproducible for the same repository state and options.
- Handle headingless and malformed Markdown files gracefully without crashing.
- Make the output useful for both interactive terminal use and downstream agent/tool consumption.

## Non-Goals

- Render Markdown body content or section text beyond heading labels.
- Generate HTML, websites, static documentation portals, or TOCs.
- Replace full-text search or documentation site generators.
- Extract front matter, anchors, line numbers, or semantic summaries in the first release.
- Support `.mdx` or non-Markdown documentation formats in the first release.
- Add `.gitignore` parsing in the first release.
- Redefine `compare`, `report`, or `export` as outline-oriented commands.
- Change the repository model schema as part of this work.
- Mutate repository files or generate documentation automatically.

## Users / Stakeholders

| User / Stakeholder | Need or Impact |
|---------------------|----------------|
| Repository contributors | Need a fast way to understand both directory structure and the internal organization of documentation without opening each file manually. |
| ARS maintainers | Need a coherent additive capability that extends ARS discovery value without weakening existing command semantics. |
| AI coding agent users | Need a compact, deterministic structural view that can be loaded as planning context. |
| Technical leads | Need a quick way to inspect documentation richness and structural consistency across repo areas. |
| CI/tooling consumers | Need a stable JSON outline shape that downstream tooling can parse without screen scraping. |

## Scope

### In Scope

- A new noun-style CLI command: `ars outline`
- Whole-repository traversal by default
- Optional subtree scoping via a path argument/option
- Inline expansion of `.md` files into heading-outline children
- `text` and `json` output formats
- Tree-depth and heading-depth controls
- Deterministic ordering of filesystem and heading output
- Reuse of existing ARS/model-native ignore behavior where applicable
- Graceful handling of headingless, unreadable, or malformed Markdown files

### Out of Scope

- Extending `compare`, `report`, or `export` to become outline commands
- `.gitignore` parsing or Git-aware ignore expansion
- `.mdx`, AsciiDoc, reStructuredText, or other non-`.md` document types
- Front matter extraction, anchor generation, line-number reporting, or link analysis
- Documentation coverage scores, summary count modes, or audit scoring
- Model schema extensions or pattern syntax changes
- Any repo mutation, auto-fix, or documentation generation workflow

## Functional Requirements

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-1 | The system must provide a dedicated `ars outline` command for repository documentation outline discovery. | Must | This is a new discovery capability, not a `compare`/`report`/`export` alias. |
| FR-2 | The command must traverse the repository root by default and may be scoped to a user-specified subtree. | Must | Supports both broad discovery and focused exploration. |
| FR-3 | The command must include directories and files in a repository tree view and expand `.md` files with their Markdown headings inline beneath the file node. | Must | Combined view is the core product requirement. |
| FR-4 | The command must preserve heading document order and heading level information for extracted Markdown headings. | Must | Users must be able to distinguish `#`, `##`, `###`, and deeper levels. |
| FR-5 | The command must support `--path`, `--max-depth`, `--headings-depth`, and `--format text|json`. | Must | Matches the required public option surface for the PRD. |
| FR-6 | Heading expansion must be enabled by default for `.md` files included in the traversal scope. | Must | The default experience should deliver the combined outline without requiring an extra flag. |
| FR-7 | The command must support `text` output that is readable in a terminal and suitable for AI-agent context loading. | Must | Rendering details belong in an RFC, but the human-readable output requirement is part of the product contract. |
| FR-8 | The command must support `json` output from day one and model the result as a unified tree with explicit node types that include at least `directory`, `file`, and `heading`. | Must | ARS already treats machine-readable output as a core requirement. |
| FR-9 | Markdown files with no headings must appear as normal file nodes without causing command failure. | Must | Empty or shallow documents must be handled gracefully. |
| FR-10 | Malformed or partially parseable Markdown must not crash the command; the command must degrade gracefully and return clear feedback when content cannot be fully interpreted. | Must | Failure should be bounded and deterministic. |
| FR-11 | The command must fail clearly when the target path does not exist, cannot be read, or is outside the allowed repository scope. | Must | Error handling must be explicit and script-friendly. |
| FR-12 | The command should reuse ARS/model-native ignore handling rather than introducing new ignore semantics in the first release. | Should | Keeps the initial slice aligned with current ARS scanning behavior. |
| FR-13 | The command could later support optional include/exclude filters, summary counts, anchors, line numbers, or additional document formats, but these are not required for the first release. | Could | Captures future extensibility without expanding current scope. |

## Non-Functional Requirements

| ID | Requirement | Target | Notes |
|----|-------------|--------|-------|
| NFR-1 | Deterministic output ordering | The same repository state and command options produce stable text ordering and byte-stable JSON ordering. | Required for automation, snapshot testing, and agent consumption. |
| NFR-2 | Terminal readability | Text output is easy to scan in standard terminal environments without opening source files. | Formatting specifics belong in a later design document. |
| NFR-3 | Machine-readable stability | JSON output uses stable field names and explicit node typing suitable for downstream tooling. | Must remain presentation-independent. |
| NFR-4 | Responsiveness | The command remains practical to use on medium-sized repositories with many Markdown files. | The first release should rely on scoping and depth controls rather than heavy analysis features. |
| NFR-5 | Testability | Output behavior is suitable for snapshot/golden tests and deterministic fixture-based validation. | Important for both text and JSON contracts. |

## Constraints

- ARS remains a CLI-first product; this capability must present through the command surface rather than through a separate UI.
- ARS remains read-only for repository content; this command may inspect and report, but must not edit files.
- The feature must preserve ARS's deterministic output expectations across supported platforms.
- The first release must fit the existing product vocabulary and output conventions rather than reworking the semantics of `compare`, `report`, or `export`.
- No model-schema change is required or assumed in this PRD; this is a read/scan/report capability layered on top of repository contents.

## Assumptions

- Standard `.md` files provide enough value to justify a first release without adding `.mdx` or other document formats.
- Existing ARS/model-native ignore behavior is sufficient for the first delivery slice, and `.gitignore` support can be deferred safely.
- Text and JSON are both required from the start because ARS serves both humans and automated consumers.
- A dedicated `ars outline` command will be easier to discover and reason about than an additional mode bolted onto `compare`, `report`, or `export`.
- Follow-on design work will define renderer details, Markdown parsing strategy, and any required ADRs without changing the product intent described here.

## Dependencies

| Dependency | Owner | Status | Impact if Delayed |
|------------|-------|--------|-------------------|
| Follow-on RFC defining command behavior, output shape details, and parser/renderer design | ARS maintainers | Not started | Implementation should not begin until the design is specified. |
| Any ADRs needed to record command-surface or output-contract decisions that materially extend current ARS architecture | ARS maintainers | Not started | Implementation may proceed inconsistently without recorded decisions. |

## Edge Cases

- A scoped `--path` points to a directory with no Markdown files; the command should still return a valid tree view for that subtree.
- A scoped `--path` points directly to a Markdown file; the command should present the file and its headings without requiring directory-only input.
- A Markdown file contains no headings; the file should be shown without heading children.
- A Markdown file contains malformed heading syntax or mixed content that prevents full interpretation; the command should degrade gracefully rather than aborting the full run.
- Files or directories cannot be read because of permissions or transient filesystem errors; the command should return clear failure behavior.
- Deep repositories or deeply nested documents produce large outputs; `--max-depth` and `--headings-depth` must bound output volume predictably.

## Acceptance Criteria

- [ ] Given a repository root with Markdown files, when `ars outline` runs with default options, then it outputs a combined tree that includes `.md` files and their headings inline.
- [ ] Given a valid subtree path, when `ars outline --path <subtree>` runs, then traversal is limited to that subtree.
- [ ] Given a tree-depth limit, when `ars outline --max-depth <n>` runs, then filesystem nodes deeper than the configured depth are omitted while shallower structure remains intact.
- [ ] Given a heading-depth limit, when `ars outline --headings-depth <n>` runs, then headings deeper than the configured depth are omitted while shallower headings remain in document order.
- [ ] Given a Markdown file with no headings, when the command runs, then the file appears in the output without heading children and without failure.
- [ ] Given malformed or partially parseable Markdown, when the command runs, then the command does not crash and produces deterministic degraded output or clear bounded feedback.
- [ ] Given `--format json`, when the command runs successfully, then the output is valid JSON representing a unified node tree with explicit node types including at least `directory`, `file`, and `heading`.
- [ ] Given an invalid, unreadable, or out-of-scope target path, when the command runs, then it exits non-zero and reports a clear error.
- [ ] Given unchanged repository contents and identical options, when the command is run repeatedly, then text output ordering and JSON output ordering remain stable.
- [ ] Given representative fixture repositories, when automated tests run, then snapshot/golden tests cover root traversal, subtree traversal, depth limits, headingless files, malformed Markdown handling, and JSON contract stability.

## Success Metrics

| Metric | Baseline | Target | Measurement Method |
|--------|----------|--------|--------------------|
| Command coverage for documentation discovery | ARS currently provides path-level discovery only | Users can inspect repo structure and Markdown section structure with one command | Manual validation against acceptance criteria and follow-on usability review during implementation |
| Machine-readable outline availability | No dedicated outline JSON contract exists | JSON outline output is available and stable in the first release of this capability | Contract tests and fixture-based snapshot validation |
| Context-loading friction for contributors and agents | Users must open Markdown files individually after locating them | Common repo-discovery workflows can begin from `ars outline` output alone | Scenario-based validation in the follow-on RFC and implementation test plan |

## Rollout Considerations

This capability is additive and should be introduced as a new command rather than a breaking change to existing behavior. No migration of existing models is required because this PRD does not introduce model-schema changes.

Rollout should preserve current ARS command semantics:

- existing `compare`, `report`, `suggest`, and `export` behavior remains unchanged
- help output should describe `ars outline` clearly and enumerate accepted option values
- JSON consumers should receive a dedicated outline contract rather than a reused comparison contract

If early implementation reveals that parser fidelity, output volume, or scanner semantics require narrower scope, the follow-on RFC should reduce implementation detail without changing the product intent captured here.

## Open Questions

- [ ] No blocking product-level open questions remain for this draft. Deferred future enhancements include `.gitignore` support, `.mdx` support, front matter extraction, anchors, line numbers, summary counts, and richer documentation analysis.

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md) | Product baseline that defines ARS's v1 mission, current command categories, and deterministic CLI-first direction. |
| [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) | Describes the current accepted CLI surface and architectural boundaries that this new command should extend rather than redefine. |
| [RFC-0003 — CLI Ergonomics and Discoverability Baseline](../rfc/RFC-0003-cli-ergonomics.md) | Establishes expectations for command discoverability, option descriptions, accepted values, and help output quality. |
| [RFC-0007 — `ars outline` Command Design](../rfc/RFC-0007-ars-outline-command-design.md) | Follow-on RFC specifying command behavior, output shapes, traversal algorithm, and parser/renderer design. |
| [ADR-0009 — Add `ars outline` to the ARS Command Surface](../adr/ADR-0009-add-ars-outline-command.md) | Records the decision to add `ars outline` as a new first-class discovery command. |
| [ADR-0010 — ATX Headings Only in v1](../adr/ADR-0010-atx-headings-only-extraction.md) | Records the decision to scope heading extraction to ATX (`#`-prefix) headings only. |
| [ADR-0011 — Flat Heading List Under File Nodes](../adr/ADR-0011-flat-heading-list-under-file-nodes.md) | Records the output contract: flat ordered heading children under file nodes with an explicit `level` field. |
