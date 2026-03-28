# ADR-0004: Keep v1 Read-Only Except for `init`

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-28 |
| **Owner(s) / Deciders** | — |
| **Related RFC** | [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-ars-v1.md) |

---

## Decision

v1 of Advanced Repo Spine is read-only with respect to repository content. The only filesystem write operation is the `init` command, which creates a new model file.

## Context

The tool's core value is inspecting and reporting on repository structure. There is a natural temptation to add auto-fix capabilities — moving misplaced files, creating missing directories, generating index files. PRD-0001 §6 explicitly excludes all mutation features from v1: no automatic folder creation, no file moves, no markdown editing, no auto-fix mode.

The decision scope is whether v1 should touch the user's repository beyond creating the initial model file.

## Options Considered

### Option 1: Read-only except `init` (Chosen)

**Description:** All commands except `init` are purely read-only. `init` writes a single new JSON model file, with overwrite protection by default.

**Pros:**
- Safe by design — the tool cannot damage repository content
- Simplifies the trust model: users can run ARS on any repo without risk
- Reduces testing surface — no need to test rollback, partial writes, or permission errors on arbitrary files
- Forces the product to prove its value through reporting and guidance before adding mutation

**Cons:**
- Users must act on findings manually — no automated remediation
- Competitive tools with auto-fix may appear more productive

### Option 2: Read-only with opt-in auto-fix for safe operations

**Description:** Add a `--fix` flag to `compare` that creates missing directories and moves clearly misplaced files with confirmation.

**Pros:**
- Higher immediate productivity for users
- Reduces friction between finding a problem and fixing it
- Competitive advantage

**Cons:**
- Mutation introduces risk: incorrect file moves, permission issues, partial failures
- Significantly more testing required (rollback, dry-run, confirmation flows, partial failure recovery)
- Misplacement detection is conservative in v1 — auto-fix on uncertain matches could cause harm
- Splits development effort between core comparison quality and mutation infrastructure

### Option 3: Full mutation support (fix, move, create, generate)

**Description:** v1 includes commands to create directories, move files, generate index files, and apply recommended fixes.

**Pros:**
- Complete workflow in a single tool
- Maximum user productivity

**Cons:**
- Dramatically increases scope — likely delays v1 significantly
- High risk of incorrect mutations before the comparison engine is proven
- Would require undo/rollback mechanisms
- Conflicts with "narrowly focused and finishable" v1 goal (PRD §27)

## Rationale

The comparison engine and model schema are unproven. Adding mutation before the core analysis is reliable would create a tool that can both misdiagnose *and* incorrectly fix — a worse outcome than misdiagnosis alone. Read-only behavior makes ARS safe to adopt incrementally: teams can integrate it into CI for reporting without risking repository damage.

The `init` exception is minimal and safe: it writes a single file with overwrite protection. This is a bootstrapping operation, not repository mutation.

Auto-fix is a strong candidate for v2 once the comparison engine is proven accurate and the model schema is stable (PRD §23).

## Consequences

### Positive

- Zero risk of repository damage from running ARS
- Simpler testing — no mutation paths to verify
- Faster v1 delivery — smaller scope
- Clear trust story: "it only looks, never touches"
- Encourages focus on comparison accuracy before adding automation

### Negative

- Users must manually act on findings
- No competitive parity with tools that offer auto-fix
- Gap between "problem detected" and "problem resolved" is entirely manual

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-ars-v1.md) | Mutation excluded in §6; safety in §20.2; over-scoping risk in §22.3 |
| [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) | Architecture assumes read-only invariant |
