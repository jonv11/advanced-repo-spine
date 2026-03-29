# CLAUDE.md — Advanced Repo Spine

## What This Repo Is

Advanced Repo Spine (ARS) is a cross-platform .NET CLI that uses a JSON model to explain, validate, and guide repository structure for humans and AI agents.

**Pitch:** The structural backbone for repositories built by humans and AI.

## Quick Orientation

| Path | Purpose |
|------|---------|
| `docs/README.md` | Documentation landing page |
| `docs/guides/documentation-quality-guide.md` | Master quality standard for all documents |
| `docs/guides/development-cycle-workflow.md` | Full development cycle workflow |
| `docs/templates/` | PRD, RFC, ADR scaffolds |
| `docs/prompts/` | Interactive AI prompts for creating PRDs, RFCs, ADRs |
| `docs/prd/` | Product Requirements Documents |
| `docs/rfc/` | Requests for Comments (design proposals) |
| `docs/adr/` | Architecture Decision Records |

## Documentation Pipeline

This project follows a strict **PRD → RFC → ADR** pipeline:

1. **PRD** — *What* and *why* (problem, goals, requirements, acceptance criteria)
2. **RFC** — *How* (design, alternatives, trade-offs, testing strategy)
3. **ADR** — *Which* and *why* (decision, context, rationale, consequences)

Documents cross-reference each other. RFCs reference PRDs. ADRs link to RFCs and PRDs.

## Naming Conventions

- `PRD-NNNN-<slug>.md` — Product Requirements Documents
- `RFC-NNNN-<slug>.md` — Requests for Comments
- `ADR-NNNN-<slug>.md` — Architecture Decision Records

Sequential numbering, zero-padded to 4 digits.

## Technical Stack

- **Runtime:** .NET, C#
- **CLI:** Spectre.Console + Spectre.Console.Cli
- **Serialization:** JSON only (`System.Text.Json`)
- **Platforms:** Windows, Linux, macOS

## Key Constraints

- v1 is read-only (except `init` which writes a starter model file)
- JSON-only model format — no YAML in v1
- Conservative misplacement detection — prefer "unmatched" over incorrect relocation claims
- Deterministic output ordering
- Separation of concerns: CLI layer, model parsing, filesystem scanning, comparison engine, reporting

## v1 CLI Commands

```
ars init       # Create starter JSON model
ars validate   # Validate JSON model
ars compare    # Compare repo against model
ars report     # Display comparison results
ars suggest    # Suggest location for a path
ars export     # Export results as JSON
ars outline    # Display repo structure with Markdown heading outlines
```

## When Writing or Editing Documents

- Read the quality guide first: `docs/guides/documentation-quality-guide.md`
- Use the matching template from `docs/templates/`
- Apply the AI-Readiness Checklist before finalizing
- Use priority language: must, should, could
- Cross-reference related PRDs, RFCs, and ADRs

## When Writing Code

- Target .NET with C#
- Use Spectre.Console for rendering, Spectre.Console.Cli for command parsing
- Use System.Text.Json for serialization
- Normalize paths before comparison
- Handle platform path separator differences
- Keep the comparison engine independently testable from the CLI shell
- Use non-zero exit codes for failures
