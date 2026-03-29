# PRD-0006: Git-Aware Text Artifact Hygiene and Ignore Policy Management

| Field | Value |
|-------|-------|
| **Status** | Draft |
| **Owner(s)** | — |
| **Date** | 2026-03-29 |
| **Stakeholders** | ARS maintainers, repository contributors, technical leads, AI coding agent users, CI/tooling consumers |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md), [PRD-0002 — Repository Documentation Outline View](PRD-0002-repository-documentation-outline-view.md), [PRD-0005 — Repository Spine Governance, Rule Enforcement, and Structural Diffing](PRD-0005-repository-spine-governance-rule-enforcement-and-structural-diffing.md), [ADR-0005 — v1 Model Schema](../adr/ADR-0005-v1-model-schema.md), [ADR-0006 — Comparison Semantics](../adr/ADR-0006-comparison-semantics.md) |

---

## Problem Statement

ARS v1 scans the filesystem without understanding Git's ignore and tracking state. When ARS compares a repository against its model, it observes every file on disk — including build artifacts, IDE configuration directories, temporary files, and generated content that Git ignores. This has two consequences: (1) ARS comparison results contain Unmatched findings for ignored files that have no structural relevance, producing noise that contributors must mentally filter; and (2) ARS cannot help maintainers audit or improve the quality of their `.gitignore` configuration.

`.gitignore` quality problems are common and create genuine repository health issues. Overly broad rules accidentally suppress valid tracked files. Duplicate or redundant rules create confusion and accumulate as maintenance cost. Rules for paths that have since been deleted are stale noise. Missing rules allow generated artifacts to enter version control — a problem that is expensive to clean up after the fact because `git rm --cached` must be run for each affected file.

Without Git-aware capabilities, ARS surfaces structural noise and no tool in the ARS ecosystem can help maintainers answer: "Is my ignore policy correctly configured?" or "Are there files in version control that should not be tracked?"

## Background / Context

[PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md) lists Git history analysis as a v1 non-goal. [PRD-0002 — Repository Documentation Outline View](PRD-0002-repository-documentation-outline-view.md) lists `.gitignore` parsing as out of scope for the `ars outline` capability. Both were deliberate deferrals.

The ARS structural model already supports a model-level `ignore` list in the `rules` section ([ADR-0005 — v1 Model Schema](../adr/ADR-0005-v1-model-schema.md)). That list is model-specific: it tells ARS which paths to skip during structural comparison, independent of Git's state. This PRD covers a separate concern: reading and analyzing Git's own ignore policy to improve ignore configuration quality and enable Git-aware filtering of ARS comparison results.

These are distinct layers. The model-level ignore list controls ARS structural scanning. The Git-level ignore policy controls what Git tracks. This PRD addresses the Git layer; it does not redefine or replace the model-level ignore behavior.

[PRD-0005 — Repository Spine Governance, Rule Enforcement, and Structural Diffing](PRD-0005-repository-spine-governance-rule-enforcement-and-structural-diffing.md) defines structural governance. The integration point between this PRD and PRD-0005 is the opt-in Git-aware comparison mode (FR-8): when active, ARS comparison filters git-ignored paths from the comparison input. That filtering reduces structural noise without changing structural governance rules.

## Goals

- Provide ARS commands that parse and analyze `.gitignore` files at the repository root and in nested directories, and report active ignore rules with source file attribution.
- Detect and report `.gitignore` rule quality issues: duplicate rules, redundant rules, and stale rules (rules that match no current file in the repository).
- Report the ignore and tracked status of files in the repository: which files are tracked, untracked, and ignored.
- Detect missing ignore patterns for configurable artifact categories and report suggested additions.
- Allow ARS structural comparisons to filter git-ignored paths from comparison input as an opt-in mode, reducing Unmatched findings caused by ignored files.
- Detect files that are tracked in Git but match common generated-artifact or should-be-ignored patterns, flagging them as potential hygiene issues for maintainer review.

## Non-Goals

- Becoming a general Git client; ARS must not wrap Git branching, merging, committing, rebasing, or remote operations.
- Repository structure governance, pattern-based structural rules, naming convention enforcement, or structural diffing — covered by PRD-0005.
- Markdown content editing, section operations, or TOC management — covered by PRD-0004.
- Front matter reading, writing, or validation — covered by PRD-0003.
- Full Git plumbing access, commit graph traversal, blame analysis, or Git history inspection.
- Integration with remote Git hosting services (GitHub, GitLab, Bitbucket) or any authentication workflow.
- Managing `.gitattributes`, `.gitmodules`, `.gitconfig`, or any Git configuration file other than `.gitignore`.
- Enforcing branch protection rules, commit message conventions, or pre-commit hook management.
- Automated remediation; ARS must never modify the Git index, run `git rm --cached`, or alter tracked file state.

## Users / Stakeholders

| User / Stakeholder | Need or Impact |
|---|---|
| Repository contributors | Need clean, noise-free ARS structural comparisons that reflect structurally relevant files rather than all filesystem contents. |
| Technical leads and maintainers | Need to detect and correct `.gitignore` quality problems and prevent generated artifacts from accumulating in version control. |
| AI coding agents | Need ARS structural comparison results filtered to tracked and structurally relevant files so that agents act on meaningful structural signals rather than filesystem noise. |
| ARS maintainers | Need a Git-aware scanning capability that integrates cleanly with the existing comparison engine without making Git a hard dependency for read-only structural analysis. |
| CI/tooling consumers | Need machine-readable ignore-policy analysis and artifact hygiene reports for automated quality gates. |

## Scope

### In Scope

- Parsing `.gitignore` files (repository root and nested directory-level) and reporting active rules with source file path and line attribution
- Detecting duplicate rules within the same `.gitignore` file (identical pattern appearing more than once)
- Detecting redundant rules (a rule whose match set is fully covered by a broader rule in the same or an ancestor ignore file)
- Detecting stale rules (rules that match no current file in the repository)
- Reporting the ignore/tracked/untracked status of files for a target path or the full repository
- Detecting missing ignore patterns for configurable artifact categories and reporting suggested additions
- Opt-in Git-aware comparison mode for `ars compare` (and related commands) that filters git-ignored paths from comparison input
- Detection of tracked files matching configurable generated-artifact patterns, reported as potential hygiene issues
- Text and JSON output formats for all analysis and hygiene reports

### Out of Scope

- Repository structure governance, structural rules, naming convention enforcement, or structural diffing
- Markdown content editing, front matter operations, or link checking
- Git history traversal, blame analysis, commit log inspection, or merge and branch operations
- Integration with remote Git hosting services
- Management of `.gitattributes`, `.gitmodules`, or other Git configuration files
- Branch protection, commit message enforcement, or hook management
- Automated remediation (ARS must not modify the Git index or run `git rm --cached`)

## Functional Requirements

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-1 | ARS must parse `.gitignore` files at the repository root and in nested directories, applying the standard `.gitignore` hierarchy (nested files take precedence over ancestor files for their subtree). | Must | The RFC must specify the parsing approach and confirm correct implementation of `.gitignore` precedence rules. |
| FR-2 | ARS must provide a command that reports the active set of `.gitignore` rules for a target path, including the source file path and line number for each rule. | Must | Both text and JSON output formats are required; JSON must use stable field names. |
| FR-3 | ARS must detect duplicate rules within the same `.gitignore` file: identical patterns appearing more than once in the same file. | Must | Reports must identify the source file and all line numbers where the duplicate appears. |
| FR-4 | ARS must detect redundant rules: rules whose matched path set is fully covered by a broader rule in the same file or an ancestor `.gitignore`. | Must | Detection must be conservative; ARS should prefer reporting as "possibly redundant" over making overconfident subsumption claims, consistent with the conservative policy in ADR-0006. |
| FR-5 | ARS must detect stale rules: rules in a `.gitignore` file that match no current file in the repository filesystem. | Must | Stale rules are a signal that the ignore configuration has not kept pace with repository evolution. |
| FR-6 | ARS must produce a report showing the ignore/tracked/untracked status of files for a target path or the full repository. | Must | Requires access to Git tracking state; the RFC must specify whether this requires a live `git` process or can be derived from Git index files. |
| FR-7 | ARS must detect missing ignore patterns for configurable artifact categories and report them as findings with suggested additions. | Must | The initial built-in categories must cover at minimum: .NET build output (`bin/`, `obj/`), OS metadata (`.DS_Store`, `Thumbs.db`), and common editor directories (`.vs/`, `.idea/`). |
| FR-8 | ARS must support an opt-in Git-aware comparison mode for `ars compare` (and related commands) that filters git-ignored paths from comparison input so that ignored files do not appear as Unmatched findings. | Must | Must be an explicit opt-in flag; default comparison behavior must remain unchanged from v1. |
| FR-9 | ARS must detect tracked files that match configurable generated-artifact patterns, flagging them as potential hygiene issues with guidance that remediation requires manual `git rm --cached` steps. | Must | ARS must only report the concern; it must never modify the Git index or execute remediation commands. |
| FR-10 | ARS should support a configurable artifact-pattern profile: a repository-specific list of patterns that define what counts as "generated" or "should-be-ignored," overriding or extending the built-in artifact category list. | Should | Allows repositories with non-standard tooling to get accurate detection. |
| FR-11 | ARS should produce an overall `.gitignore` hygiene report that summarizes all detected issues (duplicates, redundant rules, stale rules, missing patterns, potentially mistracked files) in a single command. | Should | Provides a single-command quality gate for ignore policy health. |
| FR-12 | ARS could output ready-to-paste `.gitignore` additions for detected missing patterns, formatted as valid `.gitignore` entries with inline explanatory comments. | Could | Reduces the time from detection to remediation. |
| FR-13 | ARS could detect stale-tracked files: files that are currently tracked in the Git index but are also matched by an active `.gitignore` rule, indicating that the ignore rule was added after the file was already committed. | Could | A common repository hygiene problem; ARS must only report the concern with `git rm --cached` guidance, never execute it. |

## Non-Functional Requirements

| ID | Requirement | Target | Notes |
|----|-------------|--------|-------|
| NFR-1 | Deterministic output | The same repository state and same options produce stable text and JSON output across runs. | Required for CI integration and snapshot testing. |
| NFR-2 | Machine-readable output stability | JSON field names in all output contracts remain stable across ARS releases. | Required for downstream CI parsers. |
| NFR-3 | Non-destructive operation | ARS must never modify the Git index, `.gitignore` files, or any repository content as a result of any command in this capability area. | Read-only analysis only; remediation is always the user's responsibility. |
| NFR-4 | Graceful degradation without Git | If ARS is run in a directory without a Git repository, analysis commands must report the missing Git context clearly and exit non-zero rather than crashing. | Affects FR-6 and FR-9 which depend on Git tracking state. |

## Constraints

- ARS remains CLI-first; all Git-aware capabilities must be accessible through the command surface.
- ARS must not modify the Git index, `.gitignore` files, or any tracked content; all operations are read-only analysis and reporting.
- Git-aware comparison mode must be an explicit opt-in flag to preserve backward compatibility with v1 comparison behavior.
- The RFC must determine whether Git tracking status detection (FR-6, FR-9) requires a live `git` process, reads the Git object store directly, or uses a Git library; this choice has platform support and installation dependency implications.
- `.gitignore` parsing must correctly implement the standard Git ignore hierarchy (root `.gitignore`, nested subdirectory `.gitignore` files, `.git/info/exclude`) and pattern precedence rules.
- Redundancy and stale-rule detection must be conservative; ARS should prefer "possibly redundant" over confident subsumption claims, consistent with the conservative detection principle in ADR-0006.

## Assumptions

- Repositories using ARS have a `.gitignore` file at the repository root; a missing ignore configuration is a valid finding, not a crash condition.
- The `.gitignore` format follows the standard Git specification; non-standard or hosting-platform-specific extensions are not in scope.
- Git tracking status (tracked vs. untracked vs. ignored) is useful context for ARS structural comparisons but must not be required for basic structural comparison to function.
- Artifact hygiene detection does not require heuristic content analysis; pattern-based matching against common artifact signatures is sufficient for the initial release.
- The ARS model's `rules.ignore` list (ADR-0005) is model-specific and distinct from Git's ignore state; both can coexist and serve different purposes.

## Dependencies

| Dependency | Owner | Status | Impact if Delayed |
|------------|-------|--------|-------------------|
| RFC decision on Git tracking state detection approach (live `git` process vs. Git library vs. object store reads) | ARS maintainers | Not started | Highest-risk dependency; the approach determines the Git installation requirement, platform behavior, and CI environment assumptions. |
| Follow-on RFC defining command surface, `.gitignore` parsing approach, rule quality detection algorithm, artifact category format, and hygiene report contract | ARS maintainers | Not started | Implementation must not begin without this design. |

## Edge Cases

- Repository has no `.gitignore` file: ARS must report the absence as a finding (no ignore policy configured) without crashing.
- A `.gitignore` file in a subdirectory overrides a root-level rule for its subtree: ARS must correctly apply the precedence hierarchy and attribute each rule to its source file.
- A rule pattern uses negation (`!pattern`): ARS must correctly handle negation rules and must not classify them as stale, redundant, or duplicate based solely on the `!` prefix without evaluating the full hierarchy.
- A tracked file matches an active `.gitignore` rule (stale-tracked scenario, FR-13): ARS must detect and report the anomaly; it must not attempt to remediate it.
- ARS is run outside a Git repository (e.g., in a directory that has not been initialized with `git init`): commands requiring Git state must report the missing Git context clearly and exit non-zero.
- A `.gitignore` rule uses platform-specific path separators: ARS must normalize path separators before matching, consistent with the platform-normalization policy in v1.
- A `.gitignore` file is malformed (truncated, binary content, or invalid encoding): ARS must report the parsing failure for that file without aborting the full analysis run.
- A stale-rule check is run on a repository with uncommitted new files that would be matched by an existing rule: the rule must not be flagged as stale if those files exist on the filesystem, even if they are untracked.

## Acceptance Criteria

- [ ] Given a repository with `.gitignore` files at the root and in subdirectories, when the ignore analysis command runs, then it reports all active rules with source file path and line attribution in text and JSON formats.
- [ ] Given a `.gitignore` file with duplicate rules (same pattern appearing more than once), when the analysis runs, then duplicates are reported with source file path and all duplicate line numbers.
- [ ] Given a `.gitignore` file with a rule that matches no current file in the repository, when the analysis runs, then the stale rule is identified and reported with source file path and line number.
- [ ] Given the opt-in Git-aware comparison mode flag, when `ars compare` runs, then git-ignored files do not appear as Unmatched findings in the comparison output.
- [ ] Given a repository where standard `.NET` build output directories exist but no ignore rule covers them, when the missing-pattern detection runs, then `bin/` and `obj/` are reported as missing patterns with suggested additions.
- [ ] Given a tracked file that matches a configured generated-artifact pattern, when the artifact hygiene command runs, then the file is flagged as a potential hygiene concern with guidance that remediation requires `git rm --cached`.
- [ ] Given a repository with no `.gitignore` file, when the ignore analysis runs, then it reports the absence of an ignore configuration without crashing.
- [ ] Given a directory outside a Git repository, when any command requiring Git tracking state runs, then ARS exits with a clear error identifying the missing Git context.
- [ ] Given a `.gitignore` file with a narrow rule whose match set is fully covered by a broader rule in the same file, when the analysis runs, then the narrow rule is reported as possibly redundant with source file and line number.
- [ ] Given a repository with a mix of tracked, untracked, and ignored files, when the tracked status report command runs for a target path, then it reports the ignore/tracked/untracked status of each file in the target scope in text and JSON formats.
- [ ] Given a `.gitignore` rule using a negation pattern, when the analysis runs, then the negation rule is not incorrectly classified as stale or redundant.
- [ ] Given repeated runs with identical repository state and options, then text and JSON output are stable in ordering and content.

## Success Metrics

| Metric | Baseline | Target | Measurement Method |
|--------|----------|--------|--------------------|
| `.gitignore` quality visibility | No automated ignore analysis in ARS | Duplicate, redundant, and stale rules are detectable with one command | Acceptance criteria validation |
| Structural comparison noise reduction | v1 comparison includes all filesystem files regardless of Git ignore state | Git-aware comparison mode reduces Unmatched findings caused by ignored artifact directories | Fixture-based comparison tests using repositories with known artifact directories |
| Artifact hygiene detection | No tracked-artifact detection in ARS | Commonly misconfigured tracked artifacts are detectable and reported with remediation guidance | Acceptance criteria validation; fixture-based hygiene tests |

## Rollout Considerations

Git-aware comparison mode must be an opt-in flag from the first release and must not change the default behavior of `ars compare`. This preserves backward compatibility for existing CI integrations that rely on current comparison semantics.

The RFC must resolve whether Git tracking state requires a live `git` process, which would introduce a runtime dependency on Git being installed and accessible in the path. This decision affects platform support documentation and CI environment requirements. Analysis commands that do not require Git tracking state (`.gitignore` parsing, rule quality analysis) should remain functional in repositories without a live Git context where possible, degrading gracefully when the Git-state-dependent features are invoked.

## Open Questions

- [ ] Does detecting tracked vs. ignored file status require invoking a live `git` process, or can it be derived from the Git index file (`.git/index`) without a Git installation? This is the highest-priority open question and must be resolved in the RFC before any implementation begins.
- [ ] Should ARS read `.git/info/exclude` as part of the ignore hierarchy, or limit the initial implementation to `.gitignore` files only?
- [ ] Should the opt-in Git-aware comparison mode eventually become the default, or remain opt-in indefinitely? The RFC should evaluate the backward-compatibility risk and recommend a path.

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](PRD-0001-advanced-repo-spine-v1.md) | Establishes ARS v1 scope; Git history analysis and `.gitignore` parsing are listed as v1 non-goals that this PRD addresses. |
| [PRD-0002 — Repository Documentation Outline View](PRD-0002-repository-documentation-outline-view.md) | Lists `.gitignore` parsing as out of scope for the outline capability; this PRD defines that capability as a first-class concern. |
| [PRD-0005 — Repository Spine Governance, Rule Enforcement, and Structural Diffing](PRD-0005-repository-spine-governance-rule-enforcement-and-structural-diffing.md) | Defines structural governance; the opt-in Git-aware comparison mode in this PRD (FR-8) integrates with the comparison engine that PRD-0005 extends. |
| [ADR-0005 — v1 Model Schema](../adr/ADR-0005-v1-model-schema.md) | Defines the model-level `rules.ignore` list; this is distinct from the Git-level ignore policy addressed in this PRD; both coexist for different purposes. |
| [ADR-0006 — Comparison Semantics](../adr/ADR-0006-comparison-semantics.md) | Defines the conservative detection policy that this PRD's redundancy and stale-rule detection should apply consistently. |
