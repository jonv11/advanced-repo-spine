<!--
  TEMPLATE INSTRUCTIONS
  Lines starting with "> **Guidance:**" are instructional prompts.
  Delete them as you fill in each section.
  Sections marked [Required] should always be completed.
  Sections marked [Optional] can be removed if genuinely not applicable — but consider whether they apply before removing.
-->

# PRD: [Title]

| Field | Value |
|-------|-------|
| **Status** | Draft / In Review / Approved / Implemented / Deprecated |
| **Owner(s)** | [Name(s)] |
| **Date** | YYYY-MM-DD |
| **Stakeholders** | [Names or teams who need to review or are affected] |
| **Related Links** | [Links to relevant RFCs, ADRs, Slack threads, tickets, dashboards] |

---

## Problem Statement `[Required]`

> **Guidance:** Describe the specific problem this PRD addresses. Focus on the user or business impact, not the solution. Be concrete and quantify where possible: who is affected, how often, what is the cost of inaction? A good problem statement enables a reader to evaluate whether a proposed solution actually solves the right problem.

## Background / Context `[Required]`

> **Guidance:** Provide the context a reader needs to understand why this problem matters now. Include relevant history, prior attempts, triggering events, or business drivers. Write for a reader who has no prior knowledge of this area — including AI agents that lack your organizational context.

## Goals `[Required]`

> **Guidance:** State what success looks like in concrete, measurable terms. Each goal should be verifiable after implementation. Avoid aspirational language ("improve the experience") in favor of specific outcomes ("reduce checkout abandonment rate from 12% to under 6%").

- Goal 1:
- Goal 2:

## Non-Goals `[Required]`

> **Guidance:** Explicitly state what this effort will NOT address, even if it is related or a reader might expect it to be included. Non-goals prevent scope creep and set clear expectations for implementers. If something is excluded, briefly state why.

- Non-goal 1:
- Non-goal 2:

## Users / Stakeholders `[Required]`

> **Guidance:** Identify who is affected by or benefits from this work. Include user types, internal teams, downstream systems, or external partners. For each, describe what they need or how they are impacted.

| User / Stakeholder | Need or Impact |
|---------------------|----------------|
| | |

## Scope `[Required]`

### In Scope

> **Guidance:** List what is included in this effort. Be specific enough that an implementer knows exactly what to build.

-

### Out of Scope

> **Guidance:** List what is explicitly excluded. This is as important as what is included.

-

## Functional Requirements `[Required]`

> **Guidance:** Describe what the system must do. Use consistent priority language: **Must** (non-negotiable), **Should** (important but not blocking), **Could** (desirable if feasible). Each requirement should be specific enough to implement and test without further clarification. Include examples for non-trivial requirements.

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-1 | | Must / Should / Could | |
| FR-2 | | Must / Should / Could | |

## Non-Functional Requirements `[Optional]`

> **Guidance:** Describe quality attributes the system must exhibit: performance targets (latency, throughput), availability, scalability, security, accessibility, data retention, or compliance. Each should be measurable.

| ID | Requirement | Target | Notes |
|----|-------------|--------|-------|
| NFR-1 | | | |

## Constraints `[Optional]`

> **Guidance:** List anything that limits the solution space: technology mandates, infrastructure limitations, backward compatibility requirements, regulatory requirements, budget limits, timeline constraints. Unstated constraints surface as rejected PRs or production incidents.

-

## Assumptions `[Required]`

> **Guidance:** List anything you are assuming to be true but have not verified. Assumptions that turn out to be wrong often cause the most costly rework. Be explicit so that implementers and reviewers can validate or challenge them.

-

## Dependencies `[Optional]`

> **Guidance:** Identify systems, teams, services, or timelines this work depends on. Include the nature of the dependency (blocks implementation, blocks rollout, etc.) and current status.

| Dependency | Owner | Status | Impact if Delayed |
|------------|-------|--------|-------------------|
| | | | |

## Edge Cases `[Optional]`

> **Guidance:** Describe scenarios at the boundaries of normal operation. What happens with empty inputs, maximum values, concurrent access, missing permissions, partial failures, or unexpected data? Edge cases are where implementations diverge most from intent — especially for AI agents.

-

## Acceptance Criteria `[Required]`

> **Guidance:** Define the conditions that must be true for this work to be considered complete. Each criterion should be objectively testable — a person or automated test should be able to verify it with a clear pass/fail result. Use "Given / When / Then" format where it adds clarity.

- [ ]
- [ ]
- [ ]

## Success Metrics `[Optional]`

> **Guidance:** Define how you will measure whether this effort achieved its goals after launch. Include the metric, current baseline, target, and how/when it will be measured.

| Metric | Baseline | Target | Measurement Method |
|--------|----------|--------|--------------------|
| | | | |

## Rollout Considerations `[Optional]`

> **Guidance:** Describe how this change should be released. Include feature flags, phased rollout plans, rollback procedures, migration steps, or communication plans. Address what happens if something goes wrong during rollout.

## Open Questions `[Required]`

> **Guidance:** List anything that is unresolved, uncertain, or requires further input. Even if this section is empty, include it — it forces you to consider what you do not know. For each question, note who can answer it and by when, if known.

- [ ]
- [ ]

## Related Documents `[Optional]`

> **Guidance:** Link to related PRDs, RFCs, ADRs, design docs, tickets, or external resources. Provide brief context for each link so readers know what they will find.

| Document | Relationship |
|----------|-------------|
| | |
