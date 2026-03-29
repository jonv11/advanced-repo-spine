# Capabilities

This document describes the capability families that ARS should provide to support its [target workflows](target-workflows.md). Each capability area represents a class of functionality rather than a specific command or API — the design-level details are captured in the project's RFCs and ADRs.

Capabilities are organized from foundational (structure modeling) to higher-order (context generation). Some capabilities are implemented in v1; others represent the envisioned direction. Where a capability is not yet implemented, it is described as planned or envisioned rather than current.

---

## Repository Structure Model

**Purpose:** Provide a declarative, machine-readable representation of the intended repository structure.

The structure model is the foundation of ARS. It is a JSON document that declares the intended layout of a repository: expected directories, files, nesting, optionality, and descriptive metadata. The model serves as the source of truth for repository shape and the input for all other capabilities.

Key properties of the model:

- Hierarchical and recursive, mirroring actual filesystem structure.
- Human-authorable and machine-readable.
- Versioned and deterministic.
- Supports optionality (`required` flag per item) and inline descriptions.

The model lifecycle includes initialization (generating a starter model), validation (checking the model's internal consistency), and loading (parsing and merging models for use by other capabilities).

**Supports workflows:** Repository Grounding, Repository Coverage Mapping.

**Current status:** Implemented in v1 (`ars init`, `ars validate`). See [ADR-0005](../adr/ADR-0005-v1-model-schema.md) for schema design.

---

## Structure Comparison and Drift Detection

**Purpose:** Compare the actual repository against the model and surface structural mismatches.

ARS scans the actual filesystem and compares it against the declared model. The comparison identifies items that are:

- **Missing** — expected in the model, absent from the repository.
- **Extra** — found in the repository, not represented in the model.
- **Misplaced** — found in the repository but appearing to belong elsewhere per the model.
- **Unmatched** — present but without a corresponding model expectation.

This capability is the primary mechanism for detecting structural drift: the gradual divergence between intended and actual repository shape over time.

**Supports workflows:** Repository Coverage Mapping, Overlap and Contradiction Detection.

**Current status:** Implemented in v1 (`ars compare`, `ars report`). See [ADR-0006](../adr/ADR-0006-comparison-semantics.md) for comparison semantics.

---

## Placement Guidance

**Purpose:** Suggest where a new file or folder should live, based on the structure model.

Given a file name or path, ARS consults the model to suggest the most appropriate location within the repository structure. This reduces guesswork for contributors and provides agents with deterministic placement recommendations.

**Supports workflows:** Repository Grounding, Task-Oriented Reading Plan.

**Current status:** Implemented in v1 (`ars suggest`).

---

## Machine-Readable Output

**Purpose:** Produce structured, deterministic output that can be consumed by scripts, pipelines, and AI agents.

All ARS results — comparison findings, suggestions, outlines — are available as machine-readable JSON. Output is deterministic: the same inputs produce the same output across platforms and runs. This makes ARS suitable for automation, CI integration, and agent consumption.

Human-readable output is also provided for direct use by contributors.

**Supports workflows:** Agent Context Preparation, Repository Coverage Mapping.

**Current status:** Implemented in v1 (`ars export`). See [ADR-0007](../adr/ADR-0007-report-export-aliases.md) for output conventions.

---

## Markdown-Aware Structure Extraction

**Purpose:** Surface the internal heading structure of Markdown documents as part of the repository's structural map.

ARS can extract ATX headings from Markdown files and present them as a hierarchical outline alongside the filesystem tree. This makes the internal organization of documentation visible without requiring each file to be opened and read.

This capability bridges the gap between path-level discovery ("where do documents live?") and content-level discovery ("what do they cover?").

**Supports workflows:** Document Structure Inspection, Source-of-Truth Discovery, Topic-to-Artifact Mapping.

**Current status:** Implemented in v1 (`ars outline`). See [PRD-0002](../prd/PRD-0002-repository-documentation-outline-view.md), [RFC-0007](../rfc/RFC-0007-ars-outline-command-design.md), [ADR-0009](../adr/ADR-0009-add-ars-outline-command.md), [ADR-0010](../adr/ADR-0010-atx-headings-only-extraction.md), [ADR-0011](../adr/ADR-0011-flat-heading-list-under-file-nodes.md).

---

## Source-of-Truth Awareness

**Purpose:** Help distinguish authoritative artifacts from drafts, superseded documents, and informal notes.

In documentation-driven repositories, not all documents carry equal weight. Accepted ADRs, active PRDs, and canonical guides are authoritative; rejected proposals, early drafts, and archived notes are not. ARS should support surfacing this distinction through structural conventions, model metadata, or document status signals.

This capability enables contributors and agents to identify what to trust and what to treat cautiously.

**Supports workflows:** Source-of-Truth Discovery, Task-Oriented Reading Plan, Agent Context Preparation.

**Current status:** Envisioned. The structural model provides a foundation (expected locations for authoritative artifacts), but explicit source-of-truth ranking is not yet implemented.

---

## Topic Coverage Mapping

**Purpose:** Trace a topic across documentation, decisions, and code to understand its full coverage in the repository.

A single topic — such as "output format" or "model schema" — may be addressed across PRDs (requirements), RFCs (design), ADRs (decisions), guides (conventions), and source code (implementation). ARS should support mapping a topic to its related artifacts, revealing how design intent flows from requirements through decisions to implementation.

**Supports workflows:** Topic-to-Artifact Mapping, Overlap and Contradiction Detection.

**Current status:** Envisioned. Cross-referencing between documents is a convention in this repository (RFCs reference PRDs, ADRs link to RFCs), but ARS does not yet programmatically trace these links.

---

## Conflict and Consistency Detection

**Purpose:** Identify overlap, duplication, contradiction, and staleness across repository artifacts.

As repositories grow, documentation and structural artifacts may drift from each other. Guides may contradict accepted decisions. Multiple documents may address the same topic with different conclusions. Stale artifacts may persist alongside current ones. ARS should help surface these inconsistencies.

This capability is especially important for agent-facing repositories: an agent that encounters contradictory guidance may produce incorrect or inconsistent work.

**Supports workflows:** Overlap and Contradiction Detection, Repository Coverage Mapping.

**Current status:** Envisioned. Structural comparison (missing, extra, misplaced) is implemented, but semantic conflict detection across documents is not.

---

## Context Bundle Generation

**Purpose:** Produce compact, task-appropriate context bundles for AI agents.

Before performing work, an AI agent benefits from a targeted context bundle: the structural map of the repository, the relevant source-of-truth documents, applicable conventions, and any related prior work. ARS should support generating these bundles — selecting the right subset of repository information for a given task rather than requiring the agent to load everything.

A well-constructed context bundle enables agents to act with confidence, stay within accepted conventions, and avoid duplicating existing work.

**Supports workflows:** Agent Context Preparation, Task-Oriented Reading Plan.

**Current status:** Envisioned. The structural model and comparison output provide raw material for context bundles, but task-oriented bundle generation is not yet implemented.

---

## Repository Hygiene Insights

**Purpose:** Provide a summary view of repository structural health.

ARS should support producing an at-a-glance view of repository hygiene: how well the actual structure matches the model, where the gaps are, which areas are unmodeled, and whether documentation coverage is adequate. This serves as a lightweight governance mechanism for teams that want structural oversight without heavy process.

**Supports workflows:** Repository Coverage Mapping, Overlap and Contradiction Detection.

**Current status:** Partially implemented. Comparison results (`ars compare`, `ars report`) surface structural mismatches. Higher-level hygiene summaries and trend tracking are envisioned.

---

## Further Reading

- [Software Description](../software-description.md) — what ARS is and why it exists.
- [Target Workflows](target-workflows.md) — the real-world workflows these capabilities serve.
