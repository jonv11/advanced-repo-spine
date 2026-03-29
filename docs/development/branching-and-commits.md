# Branching and Commit Conventions

This document defines the branch naming and commit message conventions for the ARS repository.

## Branch naming

Use lowercase with hyphens (kebab-case). Branches follow this format:

```
<category>/<short-description>
```

### Branch categories

| Prefix | Purpose | Example |
|--------|---------|---------|
| `feature/` | New functionality | `feature/add-watch-mode` |
| `bugfix/` | Bug fixes | `bugfix/fix-path-normalization` |
| `docs/` | Documentation-only changes | `docs/add-contributing-guide` |
| `chore/` | Maintenance, tooling, CI | `chore/update-ci-matrix` |
| `refactor/` | Code restructuring (no behavior change) | `refactor/extract-path-utils` |
| `release/` | Release preparation | `release/v1.1.0` |

### Rules

- Keep descriptions short and descriptive (2–5 words)
- Use hyphens, not underscores or slashes within the description
- The `main` branch is the primary integration branch

---

## Commit messages

This project uses [Conventional Commits](https://www.conventionalcommits.org/) (v1.0.0).

### Format

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Types

| Type | When to use |
|------|------------|
| `feat` | A new feature or user-visible capability |
| `fix` | A bug fix |
| `docs` | Documentation-only changes |
| `chore` | Maintenance, dependencies, repo config |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `test` | Adding or updating tests |
| `ci` | Changes to CI configuration or workflows |
| `build` | Changes to the build system or project files |

### Scopes (optional)

Use a scope when the change is clearly limited to one area:

| Scope | Area |
|-------|------|
| `cli` | CLI commands, argument parsing |
| `model` | JSON model parsing and validation |
| `compare` | Comparison engine |
| `scan` | Filesystem scanning |
| `report` | Reporting and output formatting |
| `suggest` | Suggestion engine |
| `outline` | Outline extraction |

### Examples

```
feat(cli): add --depth option to outline command
fix(compare): handle trailing slash in directory paths
docs: add branching and commit conventions
chore: update Spectre.Console to 0.50.0
refactor(model): extract schema validation into separate class
test(compare): add tests for case-insensitive matching
ci: add macOS to CI matrix
build: update target framework to net9.0
```

### Breaking changes

Signal breaking changes with `!` after the type/scope:

```
feat(model)!: rename 'items' to 'children' in model schema
```

Or use a `BREAKING CHANGE:` footer:

```
feat(cli): change default output format to JSON

BREAKING CHANGE: The default output format for `ars compare` is now JSON
instead of text. Use `--format text` for the previous behavior.
```

### Commit message rules

- Use the imperative mood in the description ("add feature", not "added feature")
- Do not capitalize the first letter of the description
- Do not end the description with a period
- Keep the first line under 72 characters
- Use the body for additional context when the description alone is not sufficient
- Reference related issues in the footer: `Closes #42` or `Refs #42`

---

## Pull request titles

PR titles should follow the same Conventional Commits format as commit messages:

```
feat(cli): add --depth option to outline command
docs: add release process documentation
fix(compare): correct path normalization on Windows
```

This keeps the merge commit history consistent and enables future automated changelog generation.
