# AGENTS.md — Advanced Repo Spine

## Purpose

This file provides orientation for any AI coding agent working in this repository.

## What This Repo Is

Advanced Repo Spine (ARS) is a cross-platform .NET CLI that uses a JSON model to explain, validate, and guide repository structure for humans and AI agents. v1 is read-only (except `init`) and produces deterministic, machine-readable output.

## Repository Layout

```
.github/
  copilot-instructions.md            # GitHub Copilot-specific instructions
  prompts/                           # Reusable prompt files
docs/
  README.md                          # Documentation landing page
  software-description.md            # Product positioning
  guides/
    documentation-quality-guide.md   # Master quality standard
    development-cycle-workflow.md    # Development cycle workflow
  templates/
    prd-template.md                  # PRD scaffold
    rfc-template.md                  # RFC scaffold
    adr-template.md                  # ADR scaffold
  prompts/
    create-prd-document.md           # Interactive PRD creation prompt
    create-rfc-document.md           # Interactive RFC creation prompt
    create-adr-document.md           # Interactive ADR creation prompt
  prd/                               # Product Requirements Documents
  rfc/                               # Requests for Comments
  adr/                               # Architecture Decision Records
CLAUDE.md                            # Claude-specific instructions
AGENTS.md                            # This file
README.md                            # Project entry point
```

## Documentation Pipeline

**PRD → RFC → ADR** (strict ordering):

| Document | Answers | Key Sections |
|----------|---------|-------------|
| PRD | What & Why | Problem, goals, requirements, acceptance criteria |
| RFC | How | Design, alternatives, trade-offs, testing strategy |
| ADR | Which & Why | Decision, context, rationale, consequences |

Documents cross-reference each other. Always check for existing related documents before creating new ones.

## Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| PRD | `PRD-NNNN-<slug>.md` | `PRD-0001-ars-v1.md` |
| RFC | `RFC-NNNN-<slug>.md` | `RFC-0001-cli-architecture.md` |
| ADR | `ADR-NNNN-<slug>.md` | `ADR-0001-use-dotnet-for-cli.md` |

Sequential numbering, zero-padded to 4 digits.

## Technical Stack

| Component | Choice |
|-----------|--------|
| Runtime | .NET, C# |
| CLI framework | Spectre.Console + Spectre.Console.Cli |
| Serialization | JSON only (`System.Text.Json`) |
| Platforms | Windows, Linux, macOS |

## Key Constraints

- v1 is read-only (no file mutation except `init`)
- JSON-only model format
- Conservative misplacement detection
- Deterministic output ordering
- Separation of concerns: CLI, model parsing, filesystem scanning, comparison engine, reporting

## CLI Commands

```
ars init       # Create starter JSON model
ars validate   # Validate JSON model
ars compare    # Compare repo against model
ars report     # Display comparison results
ars suggest    # Suggest location for a path
ars export     # Export results as JSON
ars outline    # Display repo structure with Markdown heading outlines
```

## Agent Workflow

### When creating documentation

1. Read `docs/guides/documentation-quality-guide.md` for quality principles
2. Read `docs/guides/development-cycle-workflow.md` for pipeline workflow
3. Use the matching template from `docs/templates/`
4. Use the matching prompt from `docs/prompts/` for interactive creation
5. Apply the AI-Readiness Checklist before finalizing
6. Use priority language consistently: must, should, could

### When writing code

1. Read PRD-0001 for product requirements
2. Read any relevant RFCs for design decisions
3. Read any relevant ADRs for architectural choices
4. Use .NET with C#, Spectre.Console, System.Text.Json
5. Normalize paths before comparison
6. Keep the comparison engine independently testable
7. Use non-zero exit codes for failures

### When making decisions

- If exploring alternatives → write an RFC first
- If recording a final decision → write an ADR
- Always link back to the originating PRD
