# Development Cycle Workflow

This guide defines the end-to-end workflow for taking an idea from inception through documentation, design, decision, implementation, and validation. It connects the individual document creation processes (PRD, RFC, ADR) into a coherent pipeline with explicit triggers, quality gates, roles, and transitions.

## Overview

```
Ideation → PRD → RFC → ADR → Implementation → Validation
```

Each phase produces a specific artifact. Phases are sequential by default — later phases reference earlier ones. However, feedback loops exist: a design challenge in the RFC phase may require updating the PRD, and implementation discoveries may trigger new ADRs.

### Phase Summary

| Phase | Artifact | Answers | Trigger | Output |
|-------|----------|---------|---------|--------|
| Ideation | — | What problem exists? | Observed need, user feedback, technical debt | Problem statement rough enough to start a PRD |
| PRD | `PRD-NNNN-<slug>.md` | What & Why | Problem is clear enough to scope | Accepted requirements with acceptance criteria |
| RFC | `RFC-NNNN-<slug>.md` | How | PRD accepted, design needed | Proposed design with alternatives and trade-offs |
| ADR | `ADR-NNNN-<slug>.md` | Which & Why | Design decision finalized | Recorded decision with rationale and consequences |
| Implementation | Code, tests, config | Build it | ADR accepted, design clear | Working software matching acceptance criteria |
| Validation | Test results, reports | Does it work? | Implementation complete | Evidence that acceptance criteria are met |

---

## Phase 1: Ideation

### Purpose

Capture and clarify a problem before committing to formal documentation. Not every idea becomes a PRD — ideation is where you decide whether the problem is worth scoping.

### Activities

- Identify the problem or opportunity
- Gather initial evidence (user reports, metrics, pain points)
- Determine if this is a new effort or an extension of existing work
- Check for existing PRDs, RFCs, or ADRs that already address the area

### Exit Criteria

- The problem can be stated in 1-3 sentences
- There is a clear reason to act now (not someday)
- It is not already covered by an existing PRD

### Who

- **Human:** Identifies the problem, decides whether to proceed
- **AI agent:** Can help research prior documents, check for duplicates, draft initial problem statement

### Next Step

Create a PRD using `docs/prompts/create-prd-document.md`.

---

## Phase 2: PRD (Product Requirements Document)

### Purpose

Define *what* is needed and *why*. Capture the problem, goals, scope, requirements, and acceptance criteria with enough precision that a designer or implementer does not need to guess intent.

### Inputs

- Problem statement from ideation
- Any existing context, constraints, or user research

### Process

1. Use the interactive prompt: `docs/prompts/create-prd-document.md`
2. The prompt follows an 8-phase iterative workflow (intake → gap analysis → structured questioning → readiness dashboard → draft → self-review → refinement)
3. Work through it until the readiness dashboard shows all required sections covered

### Template

`docs/templates/prd-template.md`

### Quality Gate

Before moving to the RFC phase, the PRD must pass:

- [ ] Problem is specific and measurable — not a solution in disguise
- [ ] Goals are concrete and measurable — not aspirational
- [ ] Non-goals are explicitly listed with rationale
- [ ] Scope boundaries (in/out) are defined
- [ ] Functional requirements use priority language (must/should/could)
- [ ] Each requirement has a corresponding acceptance criterion
- [ ] Acceptance criteria are objectively testable
- [ ] Assumptions are explicit — not presented as facts
- [ ] Open questions section exists (even if empty)
- [ ] AI-Readiness Checklist passes (see quality guide, section 9)

### Status Transitions

`Draft` → `Proposed` → `Accepted`

A PRD must be `Accepted` before an RFC should reference it as the basis for design.

### Who

- **Human:** Provides problem context, business drivers, user insights, priority decisions
- **AI agent:** Drives the interactive prompt workflow, asks structured questions, drafts and self-reviews, identifies gaps

### Next Step

If the requirements need a technical design → create an RFC.
If a technical decision can be made directly from the PRD (small, obvious) → create an ADR.

---

## Phase 3: RFC (Request for Comments)

### Purpose

Propose *how* to build what the PRD defined. Evaluate design alternatives, identify trade-offs and risks, and define a testing strategy. The RFC should be detailed enough for an engineer or AI agent to implement without making significant unguided architectural decisions.

### Inputs

- Accepted PRD (referenced, not duplicated)
- Technical context and constraints

### Prerequisite

The originating PRD must be in `Accepted` status. If requirements are unclear, go back and refine the PRD first.

### Process

1. Use the interactive prompt: `docs/prompts/create-rfc-document.md`
2. The prompt follows an 8-phase iterative workflow identical in structure to the PRD prompt
3. Reference PRD requirements by ID (e.g., "per PRD FR-3")
4. Propose at least two design alternatives
5. Be explicit about trade-offs, risks, and migration concerns

### Template

`docs/templates/rfc-template.md`

### Quality Gate

Before moving to the ADR phase, the RFC must pass:

- [ ] Summary clearly states what is proposed and the key trade-off
- [ ] Goals and non-goals are explicit
- [ ] Requirements reference the originating PRD
- [ ] Proposed design is detailed enough to implement
- [ ] Data models / interfaces / contracts are specified where applicable
- [ ] At least two alternatives were considered with rejection reasons
- [ ] Trade-offs are stated honestly
- [ ] Risks are identified with mitigation strategies
- [ ] Testing strategy covers unit, integration, and acceptance testing
- [ ] Open questions section exists
- [ ] AI-Readiness Checklist passes

### Status Transitions

`Draft` → `Proposed` → `Accepted` | `Rejected` | `Withdrawn`

An RFC may also be classified as `Exploratory` if it captures forward-looking design-space analysis rather than a concrete implementation proposal. Exploratory RFCs are not approved for implementation — future concrete work must come through separate focused RFCs.

### Who

- **Human:** Provides technical context, evaluates alternatives, makes judgment calls on trade-offs
- **AI agent:** Drives the interactive prompt workflow, proposes design options, drafts detailed design sections, identifies risks

### Next Step

For each significant design decision in the RFC → create an ADR.

---

## Phase 4: ADR (Architecture Decision Record)

### Purpose

Record *which* option was chosen and *why*. An ADR captures a decision that has been made — it is not the place to explore open-ended alternatives (that is the RFC's job).

### Inputs

- Accepted RFC with design alternatives evaluated
- Or: a decision that can be made directly from the PRD (for simple, obvious choices)

### Prerequisite

The decision must actually be made. If still exploring options, write or refine an RFC first.

### Process

1. Use the interactive prompt: `docs/prompts/create-adr-document.md`
2. State the decision in one clear sentence, active voice ("Use X for Y")
3. Evaluate at least two options with pros and cons
4. Explain why the chosen option best fits the context
5. List consequences — both positive and negative

### Template

`docs/templates/adr-template.md`

### Quality Gate

Before moving to implementation, the ADR must pass:

- [ ] Decision is stated in one clear, unambiguous sentence
- [ ] Status is `Accepted`
- [ ] Context explains the trigger and constraints
- [ ] At least two options were evaluated with pros/cons
- [ ] Rationale explains why the chosen option was selected
- [ ] Consequences (positive and negative) are listed
- [ ] Decision is traceable to a PRD or RFC
- [ ] AI-Readiness gate passes

### Status Transitions

`Proposed` → `Accepted` | `Deprecated` | `Superseded`

### Who

- **Human:** Makes the final decision, validates rationale
- **AI agent:** Drives the prompt workflow, structures options and trade-offs, drafts the record

### Next Step

Implementation can begin once relevant ADRs are `Accepted`.

---

## Phase 5: Implementation

### Purpose

Build the solution described in the RFC and constrained by the ADRs, satisfying the requirements defined in the PRD.

### Inputs

- Accepted PRD (requirements and acceptance criteria)
- Accepted RFC (design and technical approach)
- Accepted ADRs (architectural decisions and constraints)

### Process

1. Read the PRD for requirements and acceptance criteria
2. Read the RFC for design details, interfaces, and contracts
3. Read all related ADRs for architectural constraints
4. Implement according to the design
5. Write tests that map to acceptance criteria
6. Document any deviations — if the design must change during implementation, update the RFC or create a new ADR

### Rules

- **Do not deviate from accepted ADRs without creating a new ADR.** If a decision turns out to be wrong, supersede it explicitly.
- **Do not add requirements not in the PRD.** If new requirements emerge, update the PRD first.
- **Do not make significant design changes not in the RFC.** If the design needs to change, update the RFC.

### Who

- **Human:** Reviews code, validates approach, approves deviations
- **AI agent:** Implements code from the RFC design, writes tests from acceptance criteria, flags deviations

### Next Step

Once implementation is complete → validate.

---

## Phase 6: Validation

### Purpose

Verify that the implementation satisfies the acceptance criteria defined in the PRD.

### Process

1. Run all tests (unit, integration, acceptance)
2. Walk through each acceptance criterion from the PRD and verify it is met
3. Verify cross-platform behavior if applicable
4. Verify deterministic output if applicable
5. Document results

### Exit Criteria

- All acceptance criteria from the PRD pass
- All tests pass
- No unresolved deviations from the RFC design
- Results are documented or captured in CI

### Who

- **Human:** Reviews validation results, signs off
- **AI agent:** Runs tests, maps results to acceptance criteria, reports gaps

---

## Feedback Loops

The pipeline is not strictly one-way. These feedback loops are expected:

### RFC → PRD

During RFC design, you may discover that PRD requirements are ambiguous, contradictory, or missing edge cases. Update the PRD, then continue the RFC.

### ADR → RFC

A decision may invalidate part of the RFC design. Update the RFC to reflect the chosen approach.

### Implementation → ADR

If an accepted decision proves unworkable during implementation, create a new ADR that supersedes the original. Link them explicitly.

### Implementation → PRD

If implementation reveals that a requirement is infeasible or needs refinement, update the PRD. This should be rare if the PRD and RFC phases were thorough.

### Validation → Implementation

If acceptance criteria are not met, return to implementation. If the criteria themselves are wrong, update the PRD.

---

## Document Registry

Maintain awareness of all documents and their relationships:

| Field | Where to find it |
|-------|------------------|
| All PRDs | `docs/prd/` |
| All RFCs | `docs/rfc/` |
| All ADRs | `docs/adr/` |
| Document status | Metadata table at top of each document |
| Cross-references | Related Documents section in each document |

### Traceability

Every RFC must reference its originating PRD. Every ADR must reference the RFC or PRD that drove the decision. Requirements in RFCs should reference PRD requirement IDs. This creates a traceable chain from problem → design → decision → implementation.

---

## Quick Reference: When to Write What

| Situation | Write |
|-----------|-------|
| New problem or opportunity identified | PRD |
| Multiple design approaches to evaluate | RFC |
| Technical decision has been made | ADR |
| Small, obvious decision with no design alternatives | ADR (skip RFC) |
| Existing decision needs to change | New ADR that supersedes the old one |
| Requirements changed after implementation started | Update PRD, then cascade to RFC/ADR as needed |
| Bug fix or minor change | Neither — standard development workflow |

---

## Checklist: Starting a New Feature Cycle

1. [ ] Problem identified and stated clearly
2. [ ] Checked for existing PRDs/RFCs/ADRs in the area
3. [ ] PRD created using interactive prompt, self-reviewed, status → Accepted
4. [ ] RFC created (if design is non-trivial), self-reviewed, status → Accepted
5. [ ] ADRs created for key decisions, status → Accepted
6. [ ] Implementation follows RFC design within ADR constraints
7. [ ] Tests map to PRD acceptance criteria
8. [ ] Validation confirms all acceptance criteria pass
