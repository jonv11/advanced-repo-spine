# Software Description

**The structural backbone for repositories built by humans and AI.**

## What ARS Is

Advanced Repo Spine (ARS) is a repository intelligence and guidance tool. It makes the intent, structure, and guidance embedded in a repository explicit, discoverable, and machine-usable — for both human contributors and AI coding agents.

ARS treats a repository as more than a collection of folders and files. It models the repository as a structured system with meaning: what belongs where, why it exists, how artifacts relate, and how the repository should be navigated and extended. The result is a deterministic, inspectable backbone that turns implicit conventions into explicit, queryable knowledge.

## Why It Exists

Most repositories contain architecture notes, decision records, proposals, requirements, runbooks, guides, and other documentation alongside source code. These artifacts are important, but they are rarely modeled, validated, or surfaced as part of the repository's structural identity.

Without an explicit structural model:

- Contributors and agents must infer conventions from scattered files.
- Important documents are hard to find and easy to overlook.
- Structure drifts from intent as the repository evolves.
- Onboarding requires manual spelunking instead of guided navigation.
- There is no reliable way to determine what is authoritative, what is stale, or what is missing.

ARS exists to close this gap. It provides a structured, machine-readable representation of repository intent that supports grounding, navigation, validation, and guidance.

## Who It Is For

- **Human contributors** who need to understand, navigate, or extend a repository confidently — especially in documentation-heavy or decision-driven projects.
- **AI coding agents** that need deterministic, compact context about repository shape, conventions, and source-of-truth artifacts before planning or implementing work.
- **Teams** that want lightweight structural governance without heavy process overhead.

## What Makes It Different

**Repository structure as a first-class design artifact.** ARS does not just scan code or lint files. It models the intended shape of an entire repository — including documentation, decisions, and operational artifacts — and makes that model inspectable and actionable.

**Dual-audience from the start.** ARS is designed equally for humans and AI agents. Its deterministic, machine-readable output serves as a grounding layer for coding agents, while its human-readable reports support contributors directly. This is not a human tool with an agent API bolted on.

**Documentation as infrastructure.** ARS treats architecture notes, ADRs, RFCs, PRDs, guides, and other documentation as first-class structural elements of the repository spine — not as secondary content that lives outside the structural model.

**Source-of-truth-aware navigation.** Beyond validating structure, ARS is designed for workflows where contributors and agents need to discover authoritative artifacts, understand what to read first, and build task-oriented context before acting.

## What It Does — At a Glance

ARS uses a declarative JSON model to describe the intended structure of a repository. From that model, it can:

- Explain the repository layout and the purpose of each structural element.
- Compare the actual repository against the model, surfacing gaps, mismatches, and undocumented areas.
- Suggest where new files and folders should live.
- Surface the internal structure of documentation (heading outlines within Markdown files).
- Export results as machine-readable JSON for automation and agent consumption.

The goal is not only validation. It is guidance: helping humans and agents answer questions like "where should this go?", "what is this folder for?", "what documentation exists for this topic?", and "does reality still match intent?"

## Where It Fits

ARS fits naturally into repositories that are documentation-driven, decision-rich, or structurally intentional — but it is useful at any stage:

- **Greenfield projects:** Define the backbone before the codebase becomes chaotic.
- **Growing repositories:** Keep structure aligned with intent as the project evolves.
- **Mature projects:** Lightweight governance for repository hygiene, discoverability, and onboarding.
- **Agent-assisted workflows:** Provide a grounding layer that agents can load before planning, implementing, or reviewing work.

## Further Reading

- [Target Workflows](product/target-workflows.md) — the real-world workflows ARS is designed to support.
- [Capabilities](product/capabilities.md) — the capability families that enable those workflows.
- [PRD-0001](prd/PRD-0001-advanced-repo-spine-v1.md) — v1 product requirements.
- [PRD-0002](prd/PRD-0002-repository-documentation-outline-view.md) — documentation outline capability.
