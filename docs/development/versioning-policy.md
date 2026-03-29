# Versioning Policy

This document defines the versioning rules for Advanced Repo Spine.

## Standard

ARS follows [Semantic Versioning 2.0.0](https://semver.org/spec/v2.0.0.html).

Version format: **MAJOR.MINOR.PATCH**

## Current version

The current version is defined in `src/Ars.Cli/Ars.Cli.csproj` in the `<Version>` property.

## When to bump each component

### PATCH (x.y.Z)

Increment for backward-compatible fixes:

- Bug fixes that do not change CLI behavior contracts
- Internal implementation improvements
- Performance improvements with no API changes
- Dependency updates that do not affect behavior

### MINOR (x.Y.0)

Increment for backward-compatible additions:

- New CLI commands
- New command options or flags
- New model fields (backward-compatible — unknown fields are ignored)
- New finding types in comparison output
- New output format options

### MAJOR (X.0.0)

Increment for breaking changes:

- Changes to existing CLI command behavior or argument semantics
- Removal of CLI commands or options
- Changes to exit code meanings
- Breaking changes to JSON model schema (required fields, renamed fields, changed semantics)
- Breaking changes to JSON export format
- Changes to comparison semantics that alter existing findings

## Git tags

Tags use the format `v{MAJOR}.{MINOR}.{PATCH}`:

```
v1.0.0
v1.1.0
v2.0.0
```

Pre-release versions append a suffix:

```
v1.1.0-alpha.1
v1.1.0-beta.1
v1.1.0-rc.1
```

## Version location

The version is maintained in a single location:

- **`src/Ars.Cli/Ars.Cli.csproj`** — the `<Version>` property

This is the source of truth. The assembly version, file version, and `--version` output all derive from this property.

## Pre-release conventions

| Suffix | Meaning |
|--------|---------|
| `-alpha.N` | Early development, unstable, may change significantly |
| `-beta.N` | Feature-complete for the target release, may have known issues |
| `-rc.N` | Release candidate, expected to be the final version unless issues are found |

## Changelog

Version history is tracked in [CHANGELOG.md](../../CHANGELOG.md) using the [Keep a Changelog](https://keepachangelog.com/) format.

Every version bump should have a corresponding CHANGELOG entry before the release is tagged.
