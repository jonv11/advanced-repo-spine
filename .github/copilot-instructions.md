# Copilot Instructions — Advanced Repo Spine

## Project Overview

Advanced Repo Spine (ARS) is a cross-platform .NET CLI that uses a JSON model to explain, validate, and guide repository structure for humans and AI agents. It is read-only (except `init`) and produces deterministic, machine-readable output.

- **Runtime:** .NET (C#)
- **CLI framework:** Spectre.Console + Spectre.Console.Cli
- **Serialization:** JSON only (`System.Text.Json`)
- **Platforms:** Windows, Linux, macOS

## Repository Structure

```
docs/
  README.md                          # Documentation landing page
  software-description.md            # Product positioning and pitch
  guides/
    documentation-quality-guide.md   # Master standard for PRDs, RFCs, ADRs
    development-cycle-workflow.md    # Full development cycle workflow
  templates/
    prd-template.md                  # PRD scaffold
    rfc-template.md                  # RFC scaffold
    adr-template.md                  # ADR scaffold
  prompts/
    create-prd-document.md           # Interactive PRD creation prompt
    create-rfc-document.md           # Interactive RFC creation prompt
    create-adr-document.md           # Interactive ADR creation prompt
  prd/
    PRD-NNNN-<slug>.md               # Product Requirements Documents
  rfc/
    RFC-NNNN-<slug>.md               # Requests for Comments
  adr/
    ADR-NNNN-<slug>.md               # Architecture Decision Records
```

## Documentation Pipeline

The project follows a strict PRD → RFC → ADR pipeline:

- **PRD** defines *what* is needed and *why* (problem, goals, requirements, acceptance criteria)
- **RFC** proposes *how* to build it (design, alternatives, trade-offs, testing strategy)
- **ADR** records *which* option was chosen and *why* (decision, context, rationale, consequences)

Cross-reference between documents — RFCs reference PRDs, ADRs link to RFCs/PRDs.

## Naming Conventions

- PRDs: `PRD-NNNN-<slug>.md` (e.g., `PRD-0001-advanced-repo-spine-v1.md`)
- RFCs: `RFC-NNNN-<slug>.md` (e.g., `RFC-0001-cli-architecture.md`)
- ADRs: `ADR-NNNN-<slug>.md` (e.g., `ADR-0001-use-dotnet-for-cli.md`)

Sequential numbering, zero-padded to 4 digits.

## Key Design Decisions

- v1 is read-only except for `init` (no file mutation)
- JSON-only model format (no YAML in v1)
- Conservative misplacement detection (prefer "unmatched" over incorrect relocation claims)
- Deterministic output ordering
- Separation of concerns: CLI layer, model parsing, filesystem scanning, comparison engine, reporting

## CLI Commands (v1)

```
ars init       # Create starter JSON model
ars validate   # Validate JSON model
ars compare    # Compare repo against model
ars report     # Display comparison results
ars suggest    # Suggest location for a path
ars export     # Export results as JSON
```

## When Writing Documentation

- Follow the quality guide at `docs/guides/documentation-quality-guide.md`
- Use the corresponding template from `docs/templates/`
- Use interactive prompts from `docs/prompts/` for AI-assisted document creation
- Apply the AI-Readiness Checklist before finalizing any document
- Use priority language consistently: must, should, could

## When Writing Code

- Target .NET with C#
- Use Spectre.Console for CLI rendering and Spectre.Console.Cli for command parsing
- Use System.Text.Json for serialization
- Normalize paths before comparison
- Handle platform path separator differences
- Keep the comparison engine independently testable from the CLI shell
- Use non-zero exit codes for failures
