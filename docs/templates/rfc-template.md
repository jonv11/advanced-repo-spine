<!--
  TEMPLATE INSTRUCTIONS
  Lines starting with "> **Guidance:**" are instructional prompts.
  Delete them as you fill in each section.
  Sections marked [Required] should always be completed.
  Sections marked [Optional] can be removed if genuinely not applicable — but consider whether they apply before removing.
-->

# RFC: [Title]

| Field | Value |
|-------|-------|
| **Status** | Draft / In Review / Accepted / Rejected / Superseded / Exploratory |
| **Target Release** | [e.g., v1, v1.x, post-v1, or n/a] |
| **Owner(s)** | [Name(s)] |
| **Reviewers** | [Names or teams expected to review] |
| **Date** | YYYY-MM-DD |
| **Related PRD** | [Link to the PRD this RFC addresses, if applicable] |
| **Related Links** | [Links to relevant ADRs, tickets, Slack threads, dashboards, prior RFCs] |

---

## Summary `[Required]`

> **Guidance:** One paragraph (3-5 sentences) that a busy engineer can read to decide whether this RFC is relevant to them. State what you are proposing, why, and the key trade-off. This is not an abstract — it should contain enough substance to be useful on its own.

## Context / Background `[Required]`

> **Guidance:** Describe the situation that makes this RFC necessary. Include relevant history, current system state, triggering events, or business drivers. A reader with no prior context should be able to understand why this proposal exists after reading this section. Reference the PRD for detailed requirements rather than restating them.

## Problem Statement `[Required]`

> **Guidance:** State the specific technical problem this RFC addresses. This may be narrower than the PRD's problem statement — focused on the technical challenge rather than the business need. Be precise about what is broken, missing, or inadequate in the current system.

## Goals `[Required]`

> **Guidance:** State what this design must achieve in concrete terms. These should be derived from (and traceable to) the PRD requirements, translated into technical objectives.

-
-

## Non-Goals `[Required]`

> **Guidance:** State what this design explicitly does not attempt to achieve, even if related. This prevents scope creep during review and implementation.

-
-

## Requirements `[Required]`

> **Guidance:** List the technical requirements this design must satisfy. Reference PRD requirements by ID where applicable. Distinguish between hard requirements (Must) and desirable properties (Should/Could).

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| R-1 | | Must / Should / Could | PRD FR-X / new |
| R-2 | | Must / Should / Could | PRD FR-X / new |

## Constraints / Invariants `[Optional]`

> **Guidance:** List technical constraints that bound the design space: backward compatibility requirements, performance budgets, infrastructure limitations, security policies, compliance mandates, or cost ceilings. Separately list invariants — properties that must remain true before, during, and after the change (e.g., "existing API consumers must not experience breaking changes").

### Constraints

-

### Invariants

-

## Proposed Design `[Required]`

> **Guidance:** Describe the proposed technical design in enough detail that an engineer or AI agent can implement it without making significant unguided architectural decisions. Include:
> - Architecture or component overview
> - Data flow (how data moves through the system)
> - Key interfaces or contracts between components
> - Behavioral specification (what happens in normal and error cases)
> - Diagrams where they add clarity (architecture, sequence, data flow)
>
> Write for an implementer, not a product manager. Be concrete: name services, specify data formats, describe protocols. Avoid vague phrases like "a caching layer" — specify what cache, what key structure, what eviction policy.

## Data Model / Interfaces / Contracts `[Optional]`

> **Guidance:** Specify data models, API contracts, interface definitions, message schemas, or configuration formats introduced or modified by this design. Use concrete representations (JSON schemas, proto definitions, SQL DDL, TypeScript interfaces). This section is critical for AI agents — explicit contracts reduce implementation ambiguity dramatically.

## Alternatives Considered `[Required]`

> **Guidance:** Describe at least two alternative approaches you evaluated. For each, explain: what the approach is, what its strengths are, and why it was not selected. Honest evaluation of alternatives strengthens the case for the proposed design and helps future engineers understand the decision landscape.

### Alternative 1: [Name]

**Description:**

**Strengths:**

**Why not selected:**

### Alternative 2: [Name]

**Description:**

**Strengths:**

**Why not selected:**

## Trade-Offs `[Required]`

> **Guidance:** State what the proposed design gains and what it gives up compared to the alternatives. Be honest about the costs. Every design involves trade-offs — making them explicit builds trust in the proposal and helps reviewers evaluate whether the trade-offs are acceptable.

| Gain | Cost |
|------|------|
| | |

## Risks `[Optional]`

> **Guidance:** Identify what could go wrong with this design and how you plan to mitigate each risk. Include technical risks (performance, reliability, security), operational risks (deployment, rollback), and organizational risks (team capacity, dependency on other teams).

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| | Low / Med / High | Low / Med / High | |

## Migration / Rollout Plan `[Optional]`

> **Guidance:** Describe how to get from the current state to the proposed state. Include phased rollout steps, feature flags, data migration procedures, backward compatibility strategies, and rollback procedures. Address what happens if something goes wrong at each stage.

## Testing / Validation Strategy `[Required]`

> **Guidance:** Describe how the implementation will be validated. Cover:
> - **Unit tests:** What components will be tested in isolation and what behavior is verified
> - **Integration tests:** What interactions between components are tested
> - **Acceptance tests:** How you verify the design satisfies the PRD's acceptance criteria
> - **Performance tests:** How you verify performance targets are met (if applicable)
> - **Manual validation:** Any testing that cannot be automated
>
> An AI agent implementing this design should know exactly what tests to write.

## Observability / Monitoring `[Optional]`

> **Guidance:** Describe what metrics, logs, traces, alerts, or dashboards are needed to operate the system after deployment. Include: what to monitor, what thresholds trigger alerts, and what runbook steps operators should follow. This section prevents the common failure mode of shipping a feature with no way to tell if it is working.

## Security / Compliance / Cost Considerations `[Optional]`

> **Guidance:** Address security implications (authentication, authorization, data access, encryption), compliance requirements (data residency, retention, audit), and cost impact (infrastructure, licensing, operational). If none apply, briefly state why.

## Open Questions `[Required]`

> **Guidance:** List anything unresolved that could affect the design or implementation. For each, note who can answer it, the impact of not resolving it, and a proposed deadline. Even if empty, include this section — it forces you to consider what you do not know.

- [ ]
- [ ]

## Decision Outcome / Next Steps `[Optional]`

> **Guidance:** After the RFC is reviewed, record the outcome here: approved, approved with modifications, rejected, or deferred. List concrete next steps with owners and timelines. If decisions made during review changed the design, note them here or create ADRs for significant decisions.

## Related Documents `[Optional]`

> **Guidance:** Link to related PRDs, ADRs, RFCs, design docs, tickets, or external resources. Provide brief context for each link.

| Document | Relationship |
|----------|-------------|
| | |
