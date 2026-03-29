# Contributing to Advanced Repo Spine

Thank you for your interest in contributing to ARS. This guide covers what you need to get started.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later
- A Git client
- A text editor or IDE with C# support (Visual Studio, VS Code with C# Dev Kit, Rider)

## Quick start

```bash
# Clone the repository
git clone <repo-url>
cd advanced-repo-spine

# Build the solution
dotnet build Ars.sln

# Run tests
dotnet test Ars.sln

# Run the CLI
dotnet run --project src/Ars.Cli -- --help
```

## Project structure

```
src/Ars.Cli/          # CLI application (Spectre.Console)
tests/Ars.Cli.Tests/  # xUnit test suite
docs/                 # Documentation (PRDs, RFCs, ADRs, guides)
```

## Development workflow

### Before you start

1. Check existing [issues](../../issues) and [PRDs](docs/prd/) to avoid duplicating work
2. For substantial changes, open an issue or draft a PRD first
3. Create a branch following the [branch naming conventions](docs/development/branching-and-commits.md)

### Making changes

1. Create a feature branch from `main`
2. Make your changes with clear, [conventional commits](docs/development/branching-and-commits.md)
3. Ensure all tests pass: `dotnet test Ars.sln`
4. Ensure the build succeeds in Release mode: `dotnet build Ars.sln -c Release`
5. Update documentation if your change affects CLI behavior, model format, or public contracts

### Submitting a pull request

1. Push your branch and open a pull request against `main`
2. Fill out the [PR template](.github/PULL_REQUEST_TEMPLATE.md)
3. Ensure CI passes
4. Address review feedback

## Development guides

Detailed development conventions are documented in `docs/development/`:

- [Branching and commits](docs/development/branching-and-commits.md) — branch naming, commit message format
- [Versioning policy](docs/development/versioning-policy.md) — Semantic Versioning rules
- [Release process](docs/development/release-process.md) — how releases are prepared and published
- [GitHub workflow](docs/development/github-workflow.md) — issue triage, PR review, labels

## Documentation pipeline

This project follows a **PRD → RFC → ADR** documentation pipeline for significant changes:

- **PRD** (Product Requirements Document) — defines *what* and *why*
- **RFC** (Request for Comments) — proposes *how*
- **ADR** (Architecture Decision Record) — records the final *decision*

Not every change needs this pipeline. See the [GitHub workflow guide](docs/development/github-workflow.md) for when to use it.

For documentation standards, see the [documentation quality guide](docs/guides/documentation-quality-guide.md).

## Code conventions

- Target .NET 8.0 with C#
- Use Spectre.Console for CLI rendering and Spectre.Console.Cli for command parsing
- Use System.Text.Json for serialization (no YAML in v1)
- Enable nullable reference types
- Normalize paths before comparison
- Keep the comparison engine independently testable from the CLI shell
- Use non-zero exit codes for failures (see `ExitCodes.cs`)

## Testing

Tests use xUnit. The test project mirrors the source project structure:

```
tests/Ars.Cli.Tests/
  Commands/       # Command-level tests
  Comparison/     # Comparison engine tests
  Model/          # Model parsing tests
  Scanning/       # Filesystem scanning tests
  Suggestion/     # Suggestion engine tests
  Outline/        # Outline extraction tests
```

Run all tests:

```bash
dotnet test Ars.sln --verbosity normal
```

## Questions?

See the [support guide](.github/SUPPORT.md) for how to get help.
