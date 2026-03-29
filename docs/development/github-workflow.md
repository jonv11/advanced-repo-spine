# GitHub Workflow

This document describes how issues, pull requests, and the documentation pipeline interact in the ARS repository.

## Issue workflow

### When to open an issue

- **Bug reports** — unexpected behavior, crashes, incorrect output
- **Feature requests** — new capabilities, CLI improvements, model enhancements
- **Questions** — usage questions, clarification about behavior
- **Chores** — dependency updates, CI improvements, tooling changes

### Labels

| Label | Purpose |
|-------|---------|
| `bug` | Confirmed or suspected bug |
| `feature` | New capability or enhancement |
| `docs` | Documentation improvement |
| `chore` | Maintenance, tooling, CI |
| `breaking` | Involves a breaking change |
| `good first issue` | Suitable for new contributors |
| `wontfix` | Intentionally not addressing |
| `duplicate` | Duplicates an existing issue |

### Triage expectations

- New issues should be labeled within a reasonable timeframe
- Bug reports should be confirmed or marked as needing more information
- Feature requests for substantial changes should reference or trigger a PRD

---

## Pull request workflow

### When a PR is sufficient

A direct PR (without PRD/RFC/ADR) is appropriate for:

- Bug fixes
- Small, self-contained improvements
- Documentation corrections or additions
- Dependency updates
- CI and tooling changes
- Refactoring with no behavior changes

### When the documentation pipeline is needed

Use the **PRD → RFC → ADR** pipeline when:

- Adding a new CLI command
- Changing the JSON model schema
- Changing comparison semantics or finding types
- Making architectural changes
- Introducing new dependencies or replacing existing ones
- Any change that affects the public contract (CLI arguments, exit codes, output format)

See the [development cycle workflow](../guides/development-cycle-workflow.md) for the full pipeline process.

### PR expectations

1. **One concern per PR** — avoid mixing unrelated changes
2. **Descriptive title** — follow [Conventional Commits format](branching-and-commits.md)
3. **Complete PR template** — fill out all relevant sections
4. **Tests** — include tests for bug fixes and new features
5. **Documentation** — update docs if behavior changes
6. **CI green** — all checks must pass before merge

### Review process

- All PRs to `main` should be reviewed before merging
- Reviewers should check for correctness, test coverage, and documentation
- Use "Request changes" for blocking issues, "Comment" for suggestions
- Authors address all review comments before merging

### Merge strategy

- Prefer **squash merge** for feature branches (clean single-commit history)
- Use **merge commit** for release branches (preserve release preparation history)
- Delete the source branch after merging

---

## Branch protection

The following protections are configured on `main`:

- Pull request required before merging (no direct pushes to `main`)
- 1 required approving review (enforced via CODEOWNERS — `@jonv11`)
- Required status checks: `Build & Test (ubuntu-latest)`, `Build & Test (windows-latest)`, `Build & Test (macos-latest)`
- Branches must be up to date before merging
- Force-push blocked
- Branch deletion blocked
- Conversations must be resolved before merging
