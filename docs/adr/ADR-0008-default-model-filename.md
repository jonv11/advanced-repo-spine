# ADR-0008: Use `ars.json` as the Default Model File Name

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-28 |
| **Owner(s) / Deciders** | — |
| **Related RFC** | [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |

---

## Decision

Use `ars.json` (visible file) as the default model file name for all CLI commands.

## Context

Every CLI command that loads a model needs a default file name when `--model` is not specified. PRD §11 examples consistently use `ars.json`. The alternative is `.ars.json` (a hidden/dotfile), which is a common convention for configuration files on Unix-like systems.

## Options Considered

### Option 1: `ars.json` (Chosen)

**Description:** A visible file in the repository root named `ars.json`.

**Pros:**
- Immediately visible in directory listings and file explorers on all platforms
- Matches PRD §11 examples — no documentation inconsistency
- Discoverable by humans browsing the repo and by AI agents scanning for project files
- Windows does not have a dotfile convention — visible files are the norm
- Easier to find in IDE file trees without enabling "show hidden files"

**Cons:**
- Adds a visible file to the repo root (some projects prefer a clean root)
- Cannot be hidden from casual directory listings

### Option 2: `.ars.json` (hidden dotfile)

**Description:** A hidden dotfile in the repository root.

**Pros:**
- Follows Unix convention for configuration files (`.eslintrc`, `.prettierrc`)
- Reduces root directory clutter

**Cons:**
- Hidden by default on Unix — users may not realize the file exists
- Windows does not natively treat dotfiles as hidden — inconsistent cross-platform behavior
- AI agents may not scan hidden files by default
- Contradicts PRD §11 examples
- Less discoverable for new users

## Rationale

Discoverability is a core design value for ARS — the tool exists to make repository structure explicit and inspectable. A visible model file reinforces this principle. The PRD already uses `ars.json` in all examples, so adopting it as the default avoids inconsistency. The Unix dotfile convention serves tools that should stay out of the way; ARS's model file is a first-class project artifact that humans and AI agents should see and interact with.

## Consequences

### Positive

- Zero ambiguity — `ars init` creates `ars.json`, `ars validate` looks for `ars.json`
- Consistent with all PRD and RFC documentation
- AI agents reliably discover the model file without special configuration

### Negative

- One more visible file in the repo root
- Projects with strict root-cleanliness preferences may want to relocate it (supported via `--model` flag)

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | CLI examples in §11 use `ars.json` |
| [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) | Default `--model` path set to `ars.json` |
