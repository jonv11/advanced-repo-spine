# Advanced Repo Spine

**The structural backbone for repositories built by humans and AI.**

Advanced Repo Spine (ARS) is a cross-platform .NET CLI that uses a JSON model to explain, validate, and guide repository structure for humans and AI agents.

## What It Does

- **Define** intended repository structure in a JSON model
- **Compare** actual repo layout against the model
- **Report** missing, extra, misplaced, and unmatched items
- **Suggest** where new files and folders should live
- **Export** results as machine-readable JSON for automation and AI agents
- **Outline** repository structure with Markdown heading outlines

## v1 CLI Commands

```
ars init       # Create a starter JSON model
ars validate   # Validate the model
ars compare    # Compare repo against model
ars report     # Display comparison results
ars suggest    # Suggest location for a path
ars export     # Export results as JSON
ars outline    # Display repo structure with Markdown heading outlines
```

## Documentation

See [`docs/README.md`](docs/README.md) for the full documentation landing page, including:

- [Documentation Quality Guide](docs/guides/documentation-quality-guide.md) — master quality standard
- [Development Cycle Workflow](docs/guides/development-cycle-workflow.md) — full development pipeline
- [Templates](docs/templates/) — PRD, RFC, ADR scaffolds
- [Interactive Prompts](docs/prompts/) — AI-assisted document creation

## Project Status

**Phase:** v1.0

The CLI is functional with 7 commands. See [PRD-0001](docs/prd/PRD-0001-advanced-repo-spine-v1.md).

## Technical Stack

| Component | Choice |
|-----------|--------|
| Runtime | .NET, C# |
| CLI | Spectre.Console + Spectre.Console.Cli |
| Serialization | JSON (`System.Text.Json`) |
| Platforms | Windows, Linux, macOS |

## License

TBD
