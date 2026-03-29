# Target Workflows

This document describes the real-world workflows that ARS is designed to support. Each workflow represents a situation where a human contributor or AI coding agent needs to understand, navigate, or act within a repository — and where ARS should reduce guesswork, manual exploration, and risk of misstep.

These workflows define the product's purpose. The [capabilities](capabilities.md) that ARS provides are organized to serve them.

---

## Repository Grounding

**Situation:** A contributor or agent encounters a repository for the first time, or returns to one after an absence. They need to quickly understand its intended shape: what lives where, what the key areas are, and how the repository is organized.

**What ARS should help them answer:**

- What is the intended structure of this repository?
- What are the key directories and what purpose does each serve?
- Which areas contain documentation, decisions, source code, configuration, or operational artifacts?
- What is expected to exist and what is optional?

**Why it matters:** Without a grounding step, contributors and agents fall back on heuristic guessing — scanning file names, reading scattered READMEs, and inferring conventions from examples. This is slow, error-prone, and often incomplete. ARS makes the repository's intended shape directly inspectable.

---

## Source-of-Truth Discovery

**Situation:** A contributor or agent needs to find the authoritative artifact for a topic — the accepted decision, the current requirements, the active design proposal, or the canonical guide. Repositories often contain drafts, superseded documents, and parallel discussions that make it unclear which artifact is authoritative.

**What ARS should help them answer:**

- Which documents and decisions are the source of truth for a given area?
- What is the expected location for authoritative artifacts (e.g., accepted ADRs, active PRDs)?
- Are there areas where source-of-truth artifacts are expected but missing?

**Why it matters:** Acting on the wrong document — a rejected RFC, a superseded ADR, or an outdated guide — leads to rework and inconsistency. ARS should make it easier to distinguish authoritative from non-authoritative artifacts through structural conventions and model metadata.

---

## Task-Oriented Reading Plan

**Situation:** A contributor or agent is about to perform a specific task — implement a feature, draft a document, review a proposal, or refactor a subsystem. Before acting, they need to know which existing artifacts are relevant: which requirements apply, which decisions constrain the design, which guides define conventions, and whether related work already exists.

**What ARS should help them answer:**

- What should I read before starting this task?
- Which PRDs, RFCs, ADRs, guides, or code areas are relevant?
- Is there existing documentation or design work that covers this topic?
- What is the minimum set of artifacts I need to load as context?

**Why it matters:** Without a reading plan, contributors risk duplicating work, contradicting accepted decisions, or ignoring requirements. Agents in particular benefit from compact, targeted context — loading the full repository is wasteful and often exceeds context limits. ARS should help identify the smallest relevant context for a given task.

---

## Repository Coverage Mapping

**Situation:** A maintainer or reviewer wants to understand how well the repository's documentation and structure cover its intended scope. Are there declared areas with no content? Are there substantial undocumented areas? Is the model out of date relative to what actually exists?

**What ARS should help them answer:**

- Which expected areas are present and which are missing?
- Which parts of the repository exist but are not represented in the model?
- Where are the gaps between intended structure and actual content?
- How complete is the documentation coverage for a given area?

**Why it matters:** Coverage gaps compound over time. Missing documentation areas become undiscoverable. Unmodeled directories accumulate without review. ARS should surface the delta between intent and reality so that maintainers can address gaps intentionally.

---

## Topic-to-Artifact Mapping

**Situation:** A contributor or agent needs to trace a topic — such as "CLI output format" or "model schema evolution" — across the repository. The topic may be addressed across PRDs, RFCs, ADRs, guides, code, and configuration. Understanding the full picture requires finding all relevant artifacts.

**What ARS should help them answer:**

- Which artifacts in the repository relate to a given topic?
- How is a topic distributed across documentation, decisions, and code?
- Are there overlapping or contradictory artifacts on the same topic?

**Why it matters:** In documentation-driven repositories, design intent is captured across multiple artifact types. A single topic may span a PRD (requirements), an RFC (design), an ADR (decision), a guide (conventions), and source code (implementation). ARS should help trace these connections rather than forcing manual cross-referencing.

---

## Overlap, Duplication, and Contradiction Detection

**Situation:** As a repository grows, documentation and structural artifacts may drift: guides may contradict accepted ADRs, multiple documents may cover the same topic with different conclusions, or stale artifacts may persist alongside current ones.

**What ARS should help them answer:**

- Are there documents or structural areas that overlap in scope?
- Are there contradictions between accepted decisions and current guides?
- Are there stale or superseded artifacts that should be retired or updated?
- Is guidance consistent across the repository?

**Why it matters:** Contradictions and stale guidance are particularly dangerous for AI agents, which may treat any document they encounter as current. ARS should help flag structural and content-level inconsistencies so that the repository remains a reliable source of truth.

---

## Agent Context Preparation

**Situation:** An AI coding agent is about to perform work — write code, draft documentation, review a design, or validate a change. Before acting, the agent needs a compact, relevant context bundle: the repository structure, the applicable decisions and requirements, the relevant guides, and any related prior work.

**What ARS should help the agent answer:**

- What is the structural map of this repository?
- Which artifacts are relevant to the current task?
- What conventions, constraints, and decisions apply?
- What is the compact, sufficient context I need to load before acting?

**Why it matters:** AI agents have finite context windows and benefit from targeted, high-signal context rather than brute-force repository loading. ARS should support producing compact, task-appropriate context bundles that include the structural map, relevant source-of-truth artifacts, and applicable conventions — enabling agents to act with confidence rather than guesswork.

---

## Document Structure Inspection

**Situation:** A contributor or agent wants to understand not just where documentation lives, but how it is organized internally. Knowing that `docs/rfc/RFC-0001-cli-architecture.md` exists is useful; knowing its heading structure — what sections it contains and how they are arranged — provides a deeper level of discoverability without requiring the full document to be read.

**What ARS should help them answer:**

- What is the heading structure of a given Markdown document?
- How are documents organized internally across a documentation area?
- Which documents cover which subtopics, based on their section structure?

**Why it matters:** In repositories with rich documentation, the internal organization of documents is itself structural information. ARS should make this visible alongside the filesystem structure, supporting faster navigation and more informed reading decisions.

---

## Further Reading

- [Software Description](../software-description.md) — what ARS is and why it exists.
- [Capabilities](capabilities.md) — the capability families that enable these workflows.
