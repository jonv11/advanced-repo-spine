# RFC-0003 — CLI Ergonomics and Discoverability Baseline

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Target Release** | v1 |
| **Owner(s)** | — |
| **Reviewers** | — |
| **Date** | 2026-03-29 |
| **Related PRD** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |
| **Related Links** | [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md), [ADR-0002 — Use Spectre.Console](../adr/ADR-0002-use-spectre-console.md), [ADR-0008 — Default Model Filename](../adr/ADR-0008-default-model-filename.md) |

---

## Summary

This RFC defines the baseline CLI self-documentation requirements for ARS v1: every command option must have a description in help output, finite-value options must enumerate accepted values, default values must be visible, and `ars --version` must work. The scope is option-level discoverability and version discoverability. Command-level descriptions already exist in `Program.cs` and are not addressed here.

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
- G-2: Finite-value options enumerate their accepted values in help text
- G-3: Default values for options are visible in help output where relevant
- G-4: `ars --version` prints the tool version and exits with code 0
- G-5: Help text is sufficient for a new user or AI agent to construct valid commands without external reference

## Non-Goals

- Shell completion / tab-completion support
- Localization of help text into non-English languages
- Man page or `--help-all` generation
- Interactive / guided command input
- Command-level descriptions (already present in `Program.cs`)
- Adding options to commands for surface symmetry alone

---

## Requirements

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| R-1 | Every `[CommandOption]` property must have a `[Description("...")]` attribute with a meaningful description | Must | PRD-0001 §12.1 |
| R-2 | Descriptions for finite-value options must enumerate accepted values explicitly (e.g., "Output format: text or json") | Must | PRD-0001 §12.1 |
| R-3 | Descriptions must include the default value where relevant (e.g., "[default: ars.json]") | Should | PRD-0001 §12.1 |
| R-4 | `CommandApp` must be configured with `SetApplicationVersion()` so `--version` works | Must | CLI convention |
| R-5 | The version shown by `--version` must come from the project/assembly version source of truth, not a hardcoded string | Must | Maintainability |

---

## Proposed Design

### Option Descriptions

Add `[Description("...")]` attributes to all command settings properties. Recommended descriptions:

| Command | Option | Description |
|---------|--------|-------------|
| init | `--path` | Path for the output model file [default: ars.json] |
| init | `--force` | Overwrite existing file without prompting |
| validate | `--model` | Path to the JSON model file [default: ars.json] |
| validate | `--format` | Output format: text or json [default: text] |
| compare | `--model` | Path to the JSON model file [default: ars.json] |
| compare | `--root` | Root directory to compare against [default: .] |
| compare | `--format` | Output format: text or json [default: text] |
| report | `--model` | Path to the JSON model file [default: ars.json] |
| report | `--root` | Root directory to compare against [default: .] |
| suggest | `<path>` | Path or path-like hint to suggest placement for |
| suggest | `--model` | Path to the JSON model file [default: ars.json] |
| suggest | `--format` | Output format: text or json [default: text] |
| export | `--model` | Path to the JSON model file [default: ars.json] |
| export | `--root` | Root directory to compare against [default: .] |

`suggest` does not gain a `--root` option. The suggestion engine operates on the model alone and does not scan the filesystem, so `--root` would have no semantic effect. Adding options for surface symmetry alone conflicts with the project's minimal-design principles.

### Version Flag

Configure the `CommandApp` with the assembly version:

```csharp
var version = typeof(Program).Assembly
    .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()
    ?.InformationalVersion ?? "0.0.0";

app.Configure(config =>
{
    config.SetApplicationVersion(version);
});
```

The version must come from the project/assembly metadata (e.g., `<Version>` or `<InformationalVersion>` in the `.csproj`), not from a hardcoded string in `Program.cs`. This ensures `ars --version` stays in sync with the build.

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
| `--version` follows standard convention and stays in sync with the build | Requires reading version from assembly metadata |

---

## Testing / Validation Strategy

- **Unit tests:** Verify that `--help` output for each command contains description text for every option
- **Unit tests:** Verify that `ars --version` outputs a version string and exits with code 0
- **Integration test:** Invoke each command with `--help` and assert that no option appears without a description
- **Manual validation:** Review help output for each command to confirm descriptions are clear, accurate, and include defaults and accepted values

---

## Open Questions (Resolved)

- [x] **Should `--format` enumerate valid values explicitly?** Yes. Descriptions for finite-value options must enumerate accepted values (e.g., "Output format: text or json"). This enables AI agents and new users to construct valid commands from help text alone.
- [x] **Should `suggest` support `--root` for consistency?** No. The suggestion engine operates on the model, not the filesystem. A `--root` option would have no semantic effect in the current architecture and would mislead users. Options should only be added when they have a meaningful effect.

---

## Decision Outcome / Next Steps

This RFC is accepted. Implementation should:

1. Add `[Description("...")]` attributes to all command settings properties using the descriptions specified in this RFC
2. Configure `SetApplicationVersion()` using the assembly informational version
3. Ensure all finite-value options enumerate their accepted values in descriptions

---

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | User experience requirements (§12), AI-agent compatibility goals |
| [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md) | Command settings class structure and Spectre.Console.Cli usage |
| [RFC-0004 — Output Strictness](RFC-0004-output-strictness.md) | Invalid option value handling; complements discoverability with enforcement |
| [ADR-0002 — Use Spectre.Console](../adr/ADR-0002-use-spectre-console.md) | Framework choice that provides help generation |
| [ADR-0008 — Default Model Filename](../adr/ADR-0008-default-model-filename.md) | Defines `ars.json` as the default model filename |
