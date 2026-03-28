# ADR-0006: Define Comparison Semantics and Finding Types

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-28 |
| **Owner(s) / Deciders** | — |
| **Related RFC** | [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |

---

## Decision

Classify comparison findings into five types — **Missing**, **Present**, **OptionalMissing**, **Unmatched**, and **Misplaced** — using conservative misplacement detection that prefers "Unmatched" over incorrect relocation claims.

## Context

The comparison engine is the core value of ARS. It takes a validated model and a filesystem scan, then produces findings describing the relationship between the expected and actual repository structure. The design of finding types determines what information users and AI agents receive, and what actions they can take.

PRD-0001 §16 defines five finding types. RFC-0001 §Comparison Layer specifies the algorithm: flatten both trees to path sets, match, then classify unmatched items. A key tension is how aggressively to detect misplacements: if `docs/guide.md` is missing but `guide.md` exists at the root, is that a misplacement or is it an unrelated file? Incorrect misplacement claims degrade trust.

## Options Considered

### Option 1: Five finding types with conservative misplacement (Chosen)

**Description:** Findings are classified as:

| Finding Type | Meaning |
|-------------|---------|
| **Missing** | Required model item has no match in filesystem |
| **Present** | Model item has a matching filesystem entry |
| **OptionalMissing** | Optional model item (`required: false`) has no match — informational, not an error |
| **Unmatched** | Filesystem entry has no corresponding model item |
| **Misplaced** | A model item's name matches a filesystem entry but at a different path — flagged only when the match is unambiguous (single candidate, exact name match) |

Misplacement detection fires only when: (1) a model item has no direct path match, (2) exactly one scanned file has the same filename, and (3) no other model item claims that scanned file. If multiple candidates exist, all are classified as Unmatched instead.

**Pros:**
- Covers the full comparison space (expected-present, expected-absent, unexpected-present)
- Conservative misplacement avoids false positives — builds user trust
- OptionalMissing separates informational from actionable findings
- Five types are few enough to be digestible, many enough to be useful

**Cons:**
- Conservative misplacement will miss some genuine relocations (false negatives)
- No severity levels — all Missing items are equally important
- No support for "partial match" (e.g., file exists but wrong extension)

### Option 2: Three finding types (Missing, Present, Extra)

**Description:** Simpler classification — no distinction between Unmatched and Misplaced, no OptionalMissing.

**Pros:**
- Very simple to implement and understand
- No risk of incorrect misplacement claims

**Cons:**
- Loses helpful information — users must manually identify misplacements
- No way to distinguish required vs optional missing items
- Reduces value of the tool — provides less guidance than a manual `find` command

### Option 3: Scoring-based similarity matching

**Description:** Instead of discrete finding types, assign a match score (0–100%) based on path similarity, Levenshtein distance, and content hashing.

**Pros:**
- Catches subtle misplacements (renames, moves, near-matches)
- Rich information for AI agents to reason about

**Cons:**
- Non-deterministic or threshold-dependent — violates PRD §16.2 determinism requirement
- Complex to implement — content hashing is costly, similarity thresholds are arbitrary
- Harder for users to interpret — "72% match" is less actionable than "Missing"
- v1 is structure-only — content comparison is out of scope

## Rationale

Five finding types strike the right balance between simplicity and informativeness. The distinction between Missing (required, actionable) and OptionalMissing (informational) enables users to focus on what matters. The distinction between Unmatched (extra file, no guess about intent) and Misplaced (probable relocation) adds value without risking confusion — because misplacement detection is conservative.

Conservative misplacement matches PRD-0001 §1.4 ("prefer 'unmatched' over incorrect relocation claims") and RFC-0001's algorithm specification. The single-candidate-exact-name heuristic is simple, deterministic, and easy to explain. Users who want aggressive matching can use `suggest` for per-path guidance.

Scoring-based matching was rejected because it violates the determinism constraint, adds implementation complexity disproportionate to v1's scope, and produces output that is harder for both humans and AI agents to act on.

## Consequences

### Positive

- Deterministic output — same model + same files = same findings, every time
- Conservative misplacement builds trust — users learn that Misplaced findings are reliable
- Finding types map directly to exit codes (defined in ADR-0007) and report formatting
- Five types are enough for AI agents to formulate actionable recommendations
- OptionalMissing allows models to document "nice-to-have" items without false alarms

### Negative

- Some genuine misplacements will be classified as separate Missing + Unmatched pairs
- No severity or priority ordering within a finding type — all Missing items appear equally critical
- No content-aware matching — renamed files with identical content are not detected
- Future v2 enhancements (pattern matching, scoring) may require extending the finding type enum

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | Finding types defined in §16; determinism requirement in §16.2; conservatism in §1.4 |
| [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) | Comparison algorithm, `Finding` type definition, `FindingType` enum |
| [ADR-0005 — v1 Model Schema](ADR-0005-v1-model-schema.md) | Schema that the comparison engine takes as input |
| [ADR-0007 — Report and Export as Compare Aliases](ADR-0007-report-export-aliases.md) | How findings are surfaced to users |
