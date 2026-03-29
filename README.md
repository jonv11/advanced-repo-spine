# Advanced Repo Spine

**The structural backbone for repositories built by humans and AI.**

[![CI](https://github.com/jonv11/advanced-repo-spine/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/jonv11/advanced-repo-spine/actions/workflows/ci.yml)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![Latest Release](https://img.shields.io/github/v/release/jonv11/advanced-repo-spine?label=latest)](https://github.com/jonv11/advanced-repo-spine/releases)

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

**Current release:** `v1.0.0`

The CLI is functional with 7 commands. Generated models use schema version `"1.0"`. See [PRD-0001](docs/prd/PRD-0001-advanced-repo-spine-v1.md).

## Technical Stack

| Component | Choice |
|-----------|--------|
| Runtime | .NET, C# |
| CLI | Spectre.Console + Spectre.Console.Cli |
| Serialization | JSON (`System.Text.Json`) |
| Platforms | Windows, Linux, macOS |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for how to get started. Development conventions are documented in [`docs/development/`](docs/development/):

- [Branching and commits](docs/development/branching-and-commits.md)
- [Versioning policy](docs/development/versioning-policy.md)
- [Release process](docs/development/release-process.md)
- [GitHub workflow](docs/development/github-workflow.md)

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.

## License

Licensed under the [Apache License 2.0](LICENSE).
