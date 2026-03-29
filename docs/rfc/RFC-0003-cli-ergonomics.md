# RFC-0003 — CLI Ergonomics and Discoverability Baseline

| Field | Value |
|-------|-------|
| **Status** | Draft |
| **Owner(s)** | — |
| **Reviewers** | — |
| **Date** | 2026-03-29 |
| **Related PRD** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |
| **Related Links** | [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md), [ADR-0002 — Use Spectre.Console](../adr/ADR-0002-use-spectre-console.md), [ADR-0008 — Default Model Filename](../adr/ADR-0008-default-model-filename.md) |

---

## Summary

This RFC proposes a baseline set of CLI ergonomics improvements so that users and AI agents can learn ARS's capabilities from its help output alone, without reading source code or external documentation. Black-box testing found that command options lack descriptions, default values are undiscoverable from help text, and the standard `--version` flag is missing. These gaps reduce the CLI's self-documenting quality — a key requirement for both human usability and AI-agent compatibility.

---

## Context / Background

ARS v1 uses Spectre.Console.Cli for command parsing, which automatically generates help text from command definitions. However, the current command settings classes define options without `[Description]` attributes, and Spectre's help renderer does not show default values unless explicitly configured. The result is help output like:

```
OPTIONS:
    -h, --help    Prints help
        --model
        --root
        --format
```

Users cannot determine what options accept, what values are valid, or what defaults apply. The standard `--version` flag is also unsupported. These are not bugs in the framework — they are gaps in the application's configuration of it.

PRD-0001 positions ARS as a tool for both humans and AI agents. AI agents that discover CLI capabilities through `--help` output will perform poorly if options are undocumented.

---

## Problem Statement

ARS's help output does not meet baseline CLI self-documentation standards. A user encountering the tool for the first time cannot learn how to use it from `--help` alone. Specifically:

1. Options show no descriptions — users do not know what `--model`, `--root`, or `--format` expect
2. Default values are not visible — users must guess or experiment to discover that `--model` defaults to `ars.json`
3. No `--version` flag — a widely expected CLI convention is unsupported

---

## Goals

- G-1: Every command option has a human-readable description in help output
- G-2: Default values for options are visible in help output
- G-3: `ars --version` prints the tool version and exits with code 0
- G-4: Help text is sufficient for a new user or AI agent to construct valid commands without external reference

## Non-Goals

- Shell completion / tab-completion support
- Localization of help text into non-English languages
- Man page or `--help-all` generation
- Interactive / guided command input

---

## Requirements

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| R-1 | Every `[CommandOption]` property must have a `[Description("...")]` attribute with a meaningful description | Must | QOL-003 finding |
| R-2 | Descriptions must include the accepted values where the set is finite (e.g., "Output format: text or json") | Must | QOL-003, QOL-006 findings |
| R-3 | Descriptions must include the default value in brackets (e.g., "[default: ars.json]") | Should | QOL-006 finding |
| R-4 | `CommandApp` must be configured with `SetApplicationVersion()` so `--version` works | Must | QOL-001 finding |
| R-5 | The version string should follow semantic versioning | Should | Convention |

---

## Proposed Design

### Option Descriptions

Add `[Description("...")]` attributes to all command settings properties. Recommended descriptions:

| Command | Option | Description |
|---------|--------|-------------|
| init | `--path` | Path for the output model file [default: ars.json] |
| init | `--force` | Overwrite existing file without prompting |
| validate | `--model` | Path to the JSON model file [default: ars.json] |
| compare | `--model` | Path to the JSON model file [default: ars.json] |
| compare | `--root` | Root directory to compare against [default: .] |
| compare | `--format` | Output format: text or json [default: text] |
| report | `--model` | Path to the JSON model file [default: ars.json] |
| report | `--root` | Root directory to compare against [default: .] |
| suggest | `--model` | Path to the JSON model file [default: ars.json] |
| suggest | `--format` | Output format: text or json [default: text] |
| export | `--model` | Path to the JSON model file [default: ars.json] |
| export | `--root` | Root directory to compare against [default: .] |

### Version Flag

Configure the `CommandApp` with version information:

```csharp
app.Configure(config =>
{
    config.SetApplicationVersion("1.0.0");
});
```

Spectre.Console.Cli automatically registers `--version` when `SetApplicationVersion` is called.

### Default Values

Include defaults in description text as `[default: value]`. Spectre.Console.Cli does not automatically render `[DefaultValue]` attributes in help output, so encoding defaults in the description string is the most reliable approach.

---

## Alternatives Considered

### Alternative 1: External help documentation only

**Description:** Keep help text minimal; direct users to a README or man page for detailed option documentation.

**Strengths:** Less maintenance of in-code documentation.

**Why not selected:** Violates the self-documenting CLI principle. AI agents and new users should not need to find and read external docs to construct a valid command. External docs also drift from actual behavior.

### Alternative 2: Custom help renderer

**Description:** Replace Spectre's built-in help renderer with a custom implementation that reads metadata (descriptions, defaults, valid values) from a structured source.

**Strengths:** Full control over help layout and content.

**Why not selected:** Over-engineering for v1. Spectre's built-in help renderer is adequate when properly configured with descriptions. A custom renderer adds maintenance burden with minimal benefit.

---

## Trade-Offs

| Gain | Cost |
|------|------|
| Users and AI agents can learn CLI from `--help` alone | Each option needs a maintained description string |
| Default values are discoverable without experimentation | Defaults documented in two places (code + description text) |
| `--version` follows standard convention | Minor configuration change |

---

## Testing / Validation Strategy

- **Unit tests:** Verify that `--help` output for each command contains description text for every option
- **Unit tests:** Verify that `ars --version` outputs a version string and exits with code 0
- **Manual validation:** Review help output for each command to confirm descriptions are clear, accurate, and include defaults
- **Integration test:** Invoke each command with `--help` and assert that no option appears without a description

---

## Open Questions

- [ ] Should the `--format` description enumerate valid values explicitly ("text or json") or use a generic phrasing ("output format")?
- [ ] Should `suggest` support a `--root` option for consistency with `compare`/`report`/`export`, even if it is not currently used?

---

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | User experience requirements, AI-agent compatibility goals |
| [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md) | Command settings class structure and Spectre.Console.Cli usage |
| [ADR-0002 — Use Spectre.Console](../adr/ADR-0002-use-spectre-console.md) | Framework choice that provides help generation |
| [ADR-0008 — Default Model Filename](../adr/ADR-0008-default-model-filename.md) | Defines `ars.json` as the default model filename |
