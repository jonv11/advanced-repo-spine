# Software description

This software is a repository-structure intelligence and guidance tool designed primarily for software contributors and AI coding agents. Its purpose is to make the intent, organization, and expected shape of a repository explicit, discoverable, and machine-usable. Instead of treating a repository as just a loose set of folders and files, the tool models it as a structured system with meaning: what belongs where, why it exists, how artifacts relate to each other, and how contributors should navigate and extend it consistently.

At its core, the software loads one or more repository structure models, merges them into a single effective view, and compares that view against an actual working directory. From that, it can explain the repository layout, highlight gaps, detect mismatches, surface undocumented areas, and help contributors understand where new code, documents, decisions, proposals, or operational artifacts should go. The goal is not only validation, but guidance: helping humans and agents quickly answer questions such as “where should this file live?”, “what is this folder for?”, “what artifact is missing?”, or “does this repository still match its intended structure?”

The tool is especially valuable in repositories that contain more than source code. It supports the idea that architecture notes, ADRs, proposals, requirements, specs, runbooks, reference material, and contributor guidance are all part of the repository spine and should be treated as first-class structural elements. In that sense, it acts as both a structure model interpreter and a contributor-facing navigation system.

A key differentiator is that the software is intended to be AI-friendly from the start. It is not only a CLI for humans, but also a deterministic interface that coding agents can use to inspect repository intent, discover conventions, and make better placement decisions with less guesswork. Rather than forcing agents to infer everything from scattered files, the tool gives them a clearer structural map of the project. This reduces drift, improves consistency, and makes repository evolution more intentional.

The product also fits well into early-stage and evolving repositories. A team can start with a simple model, refine it over time, and use the tool to keep structure aligned with reality. In mature projects, it becomes a lightweight governance layer for repository hygiene and discoverability. In greenfield projects, it helps define the backbone before the codebase becomes chaotic.

In one sentence, this software is a structured, machine-readable backbone for understanding, validating, and guiding the shape of a repository.

## Pitch line

A strong pitch line would be:

**Make repository structure explicit, navigable, and agent-friendly.**

If you want something a bit more product-like, use:

**The structural backbone for repositories built by humans and AI.**

If you want something more practical and less marketing-like, use:

**A CLI that explains, validates, and guides repository structure.**

If you want something that emphasizes your “spine” idea more directly, use:

**Give your repository a spine.**

My recommendation for the most balanced version is:

**The structural backbone for repositories built by humans and AI.**
