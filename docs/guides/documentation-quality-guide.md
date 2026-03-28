# Writing Implementation-Ready PRDs, RFCs, and ADRs

A practical guide for engineering teams writing documentation that humans can review and AI coding agents can implement from — with minimal ambiguity and minimal back-and-forth.

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [The Documentation Pipeline: PRD, RFC, ADR](#2-the-documentation-pipeline-prd-rfc-adr)
3. [Principles of Implementation-Ready Documentation](#3-principles-of-implementation-ready-documentation)
4. [Writing for AI Coding Agents](#4-writing-for-ai-coding-agents)
5. [PRD Quality](#5-prd-quality)
6. [RFC Quality](#6-rfc-quality)
7. [ADR Quality](#7-adr-quality)
8. [Common Anti-Patterns](#8-common-anti-patterns)
9. [AI-Readiness Checklist](#9-ai-readiness-checklist)
10. [Quick Reference](#10-quick-reference)

---

## 1. Introduction

Most documentation is written for understanding. That is not enough. Documents that will drive implementation — whether by a human engineer or an AI coding agent — must be written for *action*.

The cost of ambiguity in a requirements document or design proposal is not measured in confused readers. It is measured in incorrect implementations, hidden assumptions baked into code, unnecessary back-and-forth cycles, inconsistent decisions across teams, and fragile systems that break in ways nobody anticipated.

This guide establishes a standard for writing PRDs, RFCs, and ADRs that are:

- **Precise** — say exactly what you mean, not approximately what you mean
- **Structured** — organized so any reader can find what they need without reading everything
- **Decision-oriented** — capture not just what was decided, but why, and what was rejected
- **Implementation-aware** — account for constraints, edge cases, and validation criteria
- **Testable** — define "done" in terms that can be objectively verified
- **Explicit about unknowns** — surface uncertainty rather than hiding it

**Audience:** Engineers, product managers, and technical leads who write or review PRDs, RFCs, and ADRs — especially in teams where AI coding agents perform implementation work.

**Thesis:** Good documentation quality does not mean long documentation. It means documentation that reduces the distance between intent and correct implementation to near zero.

---

## 2. The Documentation Pipeline: PRD, RFC, ADR

These three artifact types form a pipeline from need to design to decision. They complement each other; they should not duplicate each other.

| Artifact | Purpose | Core Question | Focus |
|----------|---------|---------------|-------|
| **PRD** | Define the need | *What* do we need and *why*? | Problem, requirements, value, scope, acceptance criteria |
| **RFC** | Propose a design | *How* should we build it? | Technical approach, trade-offs, alternatives, risks, rollout |
| **ADR** | Record a decision | *Which* option did we choose and *why*? | Decision, rationale, consequences, context |

### How they relate

- A **PRD** establishes the problem and requirements. It answers: what must be true when we are done?
- An **RFC** proposes how to satisfy those requirements. It references the PRD for context and evaluates design options.
- An **ADR** records a specific architectural or technical decision made during or after the RFC process. It captures the final choice, the reasoning, and the consequences.

The typical flow is **PRD → RFC → ADR**, but this is not always linear. ADRs often emerge during RFC writing when the team hits a design fork. Sometimes a PRD is written after exploratory work reveals that scope needs formal definition. The key principle: each artifact has a distinct job, and none should try to do the others' job.

### When you need each

- **Write a PRD** when there is a new product or business need that requires shared understanding of the problem, scope, and success criteria before design work begins.
- **Write an RFC** when there is a non-trivial technical design decision that affects multiple components, teams, or has significant trade-offs worth evaluating.
- **Write an ADR** when a significant technical decision has been made (or needs to be made) that future engineers will need to understand. This includes decisions made *without* a formal RFC.

### When you do NOT need one

- **Skip the PRD** for well-understood internal tooling changes where the scope is obvious and the stakeholders are the team itself.
- **Skip the RFC** for straightforward implementations where the design is obvious from the requirements and there is no meaningful alternative to evaluate.
- **Skip the ADR** for trivial decisions that are self-evident from the code or that have no meaningful alternatives.

Do not create artifacts for process compliance. Create them when the cost of misunderstanding exceeds the cost of writing.

---

## 3. Principles of Implementation-Ready Documentation

These principles apply to all three artifact types. They are the difference between documentation that informs and documentation that enables correct implementation.

### Front-load context

The first three sections of any document should answer: *What is this about? Why does it matter? What is in and out of scope?* A reader who stops after those three sections should still understand the document's purpose and boundaries.

### Be explicit about scope boundaries

What is *out of scope* matters as much as what is in scope. Unstated non-goals become assumed goals. If something is explicitly excluded, say so and say why.

### Separate requirements from suggestions

Use clear language to distinguish between what *must* happen ("must," "required," "shall") and what *should* or *could* happen ("should," "nice-to-have," "consider"). Mixing mandatory and optional language in the same paragraph creates implementation ambiguity.

### Define "done" in testable terms

Every requirement should have a corresponding acceptance criterion that an engineer (or a test) can verify. "The system should be fast" is not testable. "P95 response latency under 200ms for queries returning fewer than 1000 results" is testable.

### Make unknowns explicit

Every document should have an "Open Questions" section — even if it is empty. Forcing authors to consider what they do not know prevents hidden assumptions from reaching implementation. An open question documented is infinitely better than an assumption silently embedded in code.

### Provide rationale, not just decisions

For every significant choice in the document, state *why*. Rationale enables future engineers to evaluate whether the original reasoning still holds when circumstances change. Without rationale, decisions become unchallengeable traditions.

### Use precise terminology consistently

Define domain-specific terms on first use. Use the same term for the same concept throughout. If your organization has a glossary, link to it. If a term is overloaded (e.g., "service" could mean many things), qualify it.

### Include concrete examples

Abstract requirements become concrete through examples. "Support filtering by date range" is vague. "Support filtering by date range — for example, `?start=2025-01-01&end=2025-03-31` returns all records with `created_at` within that range, inclusive of both bounds" is implementable.

### State constraints explicitly

Every implementation operates within constraints: performance budgets, backward compatibility requirements, security policies, compliance rules, infrastructure limitations, cost ceilings. State them. Unstated constraints surface as bugs or rejected PRs.

---

## 4. Writing for AI Coding Agents

AI coding agents are increasingly used to implement work directly from documentation. They are remarkably capable — but they interpret documents differently than human engineers.

### Why AI agents need better documentation

- **Agents cannot ask clarifying questions mid-implementation.** A human engineer encountering an ambiguous requirement will walk over to someone's desk or post in Slack. An agent will make a choice — and that choice may be wrong.
- **Agents interpret literally.** If you write "the system should handle errors gracefully," an agent does not share your intuition about what "gracefully" means in your product's context. It will make a reasonable but possibly wrong interpretation.
- **Agents lack organizational context.** They do not know your team's unwritten conventions, your product's history, your users' actual behavior patterns, or your infrastructure's real-world quirks — unless you tell them.
- **Agents process structure, not nuance.** A critical constraint buried in paragraph 7 of a narrative section may be missed. The same constraint in a dedicated "Constraints" section with a clear heading will be found and respected.

### Practical principles for AI-agent-friendly documentation

1. **Use explicit headings and predictable structure.** Agents navigate documents by heading hierarchy. Use consistent heading patterns across all documents of the same type.

2. **Do not bury critical decisions in prose.** If a decision matters for implementation, give it its own heading, bullet point, or callout — not a subordinate clause in a paragraph about something else.

3. **Distinguish facts, decisions, assumptions, and open questions.** Use explicit labels. "We assume the database can handle 10K writes/second" is an assumption. "The database must handle 10K writes/second" is a requirement. These look similar in prose but have completely different implementation implications.

4. **Prefer concrete examples over abstract statements.** Instead of "support standard date formats," write "support ISO 8601 date strings (e.g., `2025-03-15T14:30:00Z`)." An agent can implement from the second; the first requires a judgment call.

5. **Define all important domain terms.** Do not assume the agent knows that "active user" means "logged in within the last 30 days" or that "workspace" refers to a specific entity in your data model.

6. **State what must remain invariant.** "Existing API consumers must not experience breaking changes" is an invariant. "The new endpoint should follow REST conventions" is a guideline. Both matter, but invariants are non-negotiable constraints that must shape every implementation choice.

7. **State what can be changed.** Explicitly identifying what is flexible gives the implementer (human or AI) appropriate latitude. "The internal data structure can be redesigned; the external API contract cannot" is extremely useful framing.

8. **Make acceptance criteria objective.** "The feature works correctly" is not an acceptance criterion. "Given a user with role `admin`, when they access `/settings`, then they see all configuration options including `billing`" is.

9. **Separate mandatory requirements from nice-to-haves.** Use explicit priority markers like `[Must]`, `[Should]`, `[Could]` or a priority column in a requirements table.

10. **Include edge cases and exception paths.** "What happens when the input is empty? When the user has no permissions? When the external service is down? When the value is at the boundary?" These are the cases that distinguish correct implementation from happy-path-only implementation.

11. **Explicitly list constraints** from infrastructure, security, performance, compliance, cost, or backward compatibility. Agents will not infer these from organizational context.

12. **Show traceability.** Link requirements to the PRD, design choices to the RFC, decisions to the ADR. An agent implementing from an RFC should be able to trace back to *why* a requirement exists.

13. **Identify what an implementer should read next.** If context is spread across multiple documents, provide a reading order. "Before implementing, read: [PRD-42](link), [ADR-17](link), and the `auth` module README."

---

## 5. PRD Quality

**Purpose:** Define the product, business, or user need. A PRD answers *what* must be built and *why* — not *how* to build it.

**Audience:** Engineers, product managers, designers, stakeholders, and AI agents that will implement or scope the work.

### What makes a PRD high quality

A high-quality PRD enables an engineer (or agent) to understand the problem deeply enough to propose a design, and to verify their implementation against clear acceptance criteria — without needing to ask the author for clarification.

It should be strong on:
- Problem definition (what is broken or missing, and for whom)
- Business/user value (why this matters now)
- Scope boundaries (what is included and what is explicitly excluded)
- Functional requirements (what the system must do)
- Acceptance criteria (how we know it works)
- Constraints (what limits the solution space)

It should be light on:
- Implementation details (that is the RFC's job)
- Technical architecture (unless it constrains requirements)
- Specific technology choices (unless mandated by the business)

### Weak vs. strong: Problem statement

**Weak:**
> We need to improve our search experience. Users have complained that search is slow and doesn't return relevant results.

**Strong:**
> Users performing product searches on the catalog page experience two problems: (1) P95 search latency is 4.2 seconds, exceeding our 1-second target by 4x, causing 23% of users to abandon the page before results load (source: Q4 analytics). (2) Searches for product names with common misspellings (e.g., "recieve" → "receive") return zero results instead of fuzzy matches, generating approximately 1,200 support tickets per month.

The strong version is specific, quantified, sourced, and gives an implementer enough context to evaluate whether a proposed solution actually addresses the problem.

### PRD quality checklist

- [ ] Problem statement describes a specific, measurable problem — not a solution
- [ ] Goals are concrete and measurable, not aspirational
- [ ] Non-goals are stated explicitly
- [ ] Scope boundaries are defined (in scope / out of scope)
- [ ] Requirements use "must" / "should" / "could" consistently
- [ ] Each functional requirement has a corresponding acceptance criterion
- [ ] Acceptance criteria are testable by a person or automated test
- [ ] Constraints (performance, security, compliance, compatibility) are listed
- [ ] Assumptions are called out explicitly
- [ ] Dependencies on other teams, systems, or timelines are identified
- [ ] Edge cases and exception scenarios are addressed
- [ ] Success metrics define what "working" looks like post-launch
- [ ] Open questions are listed, even if the list is empty

**Template:** [PRD Template](../templates/prd-template.md)

---

## 6. RFC Quality

**Purpose:** Propose and evaluate a technical design that satisfies the requirements from a PRD (or a well-understood need). An RFC answers *how* something should be built and *what trade-offs* that approach entails.

**Audience:** Engineers, architects, reviewers, and AI agents that will implement the design or evaluate its feasibility.

### What makes an RFC high quality

A high-quality RFC enables an engineer (or agent) to implement the design without making significant unguided architectural decisions — and enables a reviewer to evaluate whether the design is sound, complete, and aligned with requirements.

It should be strong on:
- Technical design (architecture, data flow, interfaces, contracts)
- Alternatives considered (what else was evaluated and why it was rejected)
- Trade-offs (what you gain and what you give up with the proposed approach)
- Risks (what could go wrong and how to mitigate it)
- Testing strategy (how the implementation will be validated)

It should be light on:
- Restating the problem in detail (reference the PRD)
- Product requirements (those belong in the PRD)
- Final decisions without context (those belong in ADRs)

### Weak vs. strong: Proposed design

**Weak:**
> We'll add a caching layer to improve search performance. The cache will store recent queries and return cached results when possible.

**Strong:**
> We propose adding a read-through cache between the search API and the search index, using Redis with a 5-minute TTL.
>
> - **Cache key:** normalized query string + user locale + pagination params, hashed with SHA-256.
> - **Cache invalidation:** TTL-based (5 min) for general queries. Immediate invalidation via pub/sub event when a product's `indexed_at` timestamp is updated.
> - **Cache miss behavior:** Request falls through to the search index; response is cached before returning to the client.
> - **Size limit:** Max 10GB, LRU eviction. Based on current query cardinality (~200K unique queries/day), this covers approximately 80% of daily queries.
> - **Failure mode:** If Redis is unavailable, the search API bypasses the cache and queries the index directly. No user-facing error.

The strong version is implementable. An engineer or agent reading it knows exactly what to build, what keys look like, how invalidation works, and what happens when things fail.

### RFC quality checklist

- [ ] Summary gives a clear, one-paragraph overview of the proposal
- [ ] Context explains why this RFC exists now (links to PRD or triggering event)
- [ ] Goals and non-goals are explicit
- [ ] Requirements are stated or referenced from the PRD
- [ ] Proposed design is detailed enough to implement without major ambiguity
- [ ] Data models, interfaces, or API contracts are specified where relevant
- [ ] At least two alternatives are considered with clear reasons for rejection
- [ ] Trade-offs of the proposed approach are stated honestly
- [ ] Risks are identified with mitigation strategies
- [ ] Migration or rollout plan addresses how to go from current state to proposed state
- [ ] Testing strategy covers unit, integration, and acceptance validation
- [ ] Operational concerns (monitoring, alerting, observability) are addressed
- [ ] Security, compliance, and cost implications are noted where relevant
- [ ] Open questions are listed
- [ ] Constraints and invariants (backward compatibility, performance budgets) are explicit

**Template:** [RFC Template](../templates/rfc-template.md)

---

## 7. ADR Quality

**Purpose:** Record a specific architectural or technical decision so that future engineers understand *what* was decided, *why*, and *what the consequences are*. An ADR answers: which option did we choose and what follows from that choice?

**Audience:** Future engineers (including future you), architects, and AI agents that need to understand why the system is designed the way it is.

### What makes an ADR high quality

A high-quality ADR enables a future engineer (or agent) to understand the decision's context well enough to evaluate whether the reasoning still holds — and to understand the consequences well enough to work within (or deliberately revisit) the decision.

It should be strong on:
- The decision itself (stated clearly and upfront)
- Context (what situation made this decision necessary)
- Options evaluated (what was on the table)
- Rationale (why this option was chosen over the others)
- Consequences (what follows from this decision, both positive and negative)

It should be light on:
- Detailed technical design (that belongs in the RFC)
- Requirements (those belong in the PRD)
- Implementation specifics (those belong in the code)

### Weak vs. strong: Decision and rationale

**Weak:**
> We decided to use PostgreSQL for the new service.

**Strong:**
> **Decision:** Use PostgreSQL 15 as the primary datastore for the Order Service.
>
> **Rationale:** We evaluated PostgreSQL, DynamoDB, and CockroachDB. PostgreSQL was selected because: (1) the Order Service requires complex transactional queries across 4+ joined tables, which PostgreSQL handles natively; (2) the team has deep operational experience with PostgreSQL (3 years in production for the Catalog Service); (3) our expected write throughput (≤5K writes/sec) is well within PostgreSQL's capacity on our current infrastructure tier. DynamoDB was rejected because the access patterns require flexible joins that would require denormalization beyond acceptable maintenance cost. CockroachDB was rejected because the multi-region consistency guarantees are not needed for this service's single-region deployment.

The strong version tells a future engineer exactly why this choice was made and under what conditions it might need revisiting (e.g., if write throughput exceeds 5K/sec or multi-region becomes a requirement).

### ADR quality checklist

- [ ] Decision is stated in one clear sentence at the top
- [ ] Status is current (Proposed / Accepted / Deprecated / Superseded)
- [ ] Context explains what situation or trigger required this decision
- [ ] At least two options were evaluated
- [ ] Each option has stated pros and cons
- [ ] Rationale explains why the chosen option best fits the context
- [ ] Consequences (positive and negative) are listed
- [ ] If this supersedes a previous ADR, that relationship is documented
- [ ] Decision is traceable to a PRD or RFC where relevant

**Template:** [ADR Template](../templates/adr-template.md)

---

## 8. Common Anti-Patterns

| Anti-Pattern | Problem | Fix |
|---|---|---|
| **The Visionary PRD** — All aspiration, no specifics. Goals like "delight users" and "best-in-class experience" with no measurable requirements. | Implementers cannot verify their work against the requirements because there are no concrete requirements. | Replace aspirational goals with measurable outcomes. Define acceptance criteria for every requirement. |
| **The Implementation RFC** — Describes code structure and function names instead of architectural decisions and trade-offs. | Tightly couples the design document to a specific implementation. Any refactor invalidates the RFC. Reviewers cannot evaluate the design separately from the code. | Focus on interfaces, data flow, contracts, and behavior. Leave internal code structure to the implementer. |
| **The Retroactive ADR** — Written after the decision was implemented, reconstructing context from memory. | Missing or inaccurate context, conveniently omitted alternatives, and post-hoc rationalization that does not reflect the actual decision process. | Write ADRs when the decision is made, not after it is implemented. Even a rough draft during the decision is better than a polished reconstruction later. |
| **The Novel** — Critical information buried in pages of narrative prose with no headings or structure. | Readers (especially AI agents) miss critical constraints, decisions, or requirements hidden in paragraph 12 of a wall of text. | Use headings, bullet points, tables, and callouts. One idea per section. Critical information gets its own heading. |
| **The Skeleton** — Template filled in with one-liners or "N/A" for most sections. | Gives the appearance of completeness while communicating almost nothing. Implementers default to assumptions the author never validated. | If a section truly does not apply, explain briefly why. If it does apply, write enough to be actionable. An empty template is honest; a skeleton is misleading. |
| **The Assumption Document** — Relies on tribal knowledge, unwritten conventions, and "everyone knows" context. | New team members, engineers from other teams, and AI agents will implement incorrectly because they do not share the assumed context. | State every assumption explicitly. Define domain terms. Link to relevant context. Write as if the reader has never worked on this system. |
| **The Copy-Paste Spec** — Requirements duplicated verbatim across PRD, RFC, and ADR with no distinction between need, design, and decision. | Changes in one document are not propagated to the others. Contradictions emerge. Readers cannot tell which document is authoritative. | Each artifact has a distinct job. The PRD states *what*, the RFC states *how*, the ADR records *which*. Cross-reference rather than duplicate. |
| **The Ambiguous Terminology** — Same word used to mean different things in different sections, or different words used for the same concept. | Implementers make inconsistent choices because they interpret terms differently in different contexts. | Define key terms once, early in the document. Use them consistently. If a term is overloaded, qualify it (e.g., "user" → "authenticated user" vs. "anonymous visitor"). |

---

## 9. AI-Readiness Checklist

Use this checklist when reviewing any document (PRD, RFC, or ADR) that will be used by an AI coding agent for implementation. Each item should be answerable with yes or no.

### Clarity and structure

- [ ] The document uses consistent heading hierarchy (H1 for title, H2 for sections, H3 for subsections)
- [ ] Critical decisions and constraints have their own headings or bullet points — not buried in prose
- [ ] Facts, decisions, assumptions, and open questions are clearly distinguished
- [ ] Domain-specific terms are defined on first use

### Scope and requirements

- [ ] Scope boundaries are explicit (in scope and out of scope)
- [ ] Requirements use consistent priority language ("must" / "should" / "could")
- [ ] Mandatory requirements are clearly separated from nice-to-haves
- [ ] Each requirement is specific enough to implement without a follow-up question
- [ ] Edge cases and exception paths are documented

### Testability

- [ ] Acceptance criteria are stated as objectively verifiable conditions
- [ ] Success metrics are measurable
- [ ] Examples are provided for non-trivial requirements

### Constraints and context

- [ ] Performance, security, compliance, and compatibility constraints are stated
- [ ] Assumptions are explicitly listed
- [ ] Dependencies on other systems, teams, or timelines are identified
- [ ] What must remain invariant is stated
- [ ] What can be changed or is flexible is stated

### Traceability and navigation

- [ ] Related documents (PRDs, RFCs, ADRs) are linked
- [ ] The document identifies what an implementer should read next
- [ ] Rationale is provided for significant choices

### Unknowns

- [ ] Open questions are listed in a dedicated section
- [ ] The document does not present assumptions as facts

---

## 10. Quick Reference

| Artifact | When to Use | Key Sections | Primary Focus |
|----------|-------------|--------------|---------------|
| **PRD** | New product/business need requiring shared understanding of scope | Problem, Goals, Requirements, Acceptance Criteria, Non-Goals | What and Why |
| **RFC** | Non-trivial technical design with meaningful trade-offs | Design, Alternatives, Trade-Offs, Risks, Testing Strategy | How |
| **ADR** | Significant architectural/technical decision to record | Decision, Context, Options, Rationale, Consequences | Which and Why |

### Templates

- [PRD Template](../templates/prd-template.md)
- [RFC Template](../templates/rfc-template.md)
- [ADR Template](../templates/adr-template.md)

### The one rule

If an engineer or AI agent reading your document must guess what you meant, the document is not ready for implementation.
