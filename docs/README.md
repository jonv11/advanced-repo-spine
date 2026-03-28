# Implementation-Ready Documentation Standards

A practical guide and reusable templates for writing PRDs, RFCs, and ADRs that are clear enough for humans to review and precise enough for AI coding agents to implement from.

## Contents

### Guides

- **[Documentation Quality Guide](guides/documentation-quality-guide.md)** — What makes PRDs, RFCs, and ADRs high quality and implementation-ready, with dedicated guidance for AI-assisted implementation, anti-patterns, and review checklists.
- **[Development Cycle Workflow](guides/development-cycle-workflow.md)** — End-to-end workflow for the full pipeline: Ideation → PRD → RFC → ADR → Implementation → Validation.

### Templates

- **[PRD Template](templates/prd-template.md)** — Product Requirements Document. Defines *what* is needed and *why*.
- **[RFC Template](templates/rfc-template.md)** — Request for Comments. Proposes *how* to build it and evaluates trade-offs.
- **[ADR Template](templates/adr-template.md)** — Architecture Decision Record. Records *which* option was chosen and *why*.

### Prompts

- **[Create PRD Document](prompts/create-prd-document.md)** — AI assistant prompt for iteratively creating a high-quality PRD.
- **[Create RFC Document](prompts/create-rfc-document.md)** — AI assistant prompt for iteratively creating a high-quality RFC.
- **[Create ADR Document](prompts/create-adr-document.md)** — AI assistant prompt for iteratively creating a high-quality ADR.

## Usage

1. Read the [Documentation Quality Guide](guides/documentation-quality-guide.md) to understand the principles.
2. Copy the relevant template into your project's documentation directory.
3. Fill in each section, following the `> **Guidance:**` prompts.
4. Delete the guidance lines before finalizing.
5. Use the [AI-Readiness Checklist](guides/documentation-quality-guide.md#9-ai-readiness-checklist) to verify the document is implementation-ready.

**Alternative: AI-assisted creation.** Use one of the [prompts](#prompts) to work interactively with an AI assistant that will guide you through creating the document step by step.

## Core Principle

If an engineer or AI agent reading your document must guess what you meant, the document is not ready for implementation.
