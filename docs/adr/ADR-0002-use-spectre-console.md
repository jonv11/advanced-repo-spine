# ADR-0002: Use Spectre.Console and Spectre.Console.Cli

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-28 |
| **Owner(s) / Deciders** | — |
| **Related RFC** | [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |

---

## Decision

Use Spectre.Console for terminal rendering and Spectre.Console.Cli for command parsing and routing.

## Context

Advanced Repo Spine needs a CLI framework that handles command parsing (six commands with varying arguments and options) and produces formatted terminal output (tables, colored text, grouped sections). The PRD mandates Spectre.Console + Spectre.Console.Cli (§13.2). The tool must produce output that is readable in CI logs as well as interactive terminals.

Spectre.Console’s markup system interprets `[...]` as style/color tags. Any dynamic content rendered via `MarkupLine` or similar methods must either escape square brackets (`[[` / `]]`) or be output through non-markup methods (e.g., `AnsiConsole.WriteLine`). Failure to do so causes framework exceptions when the dynamic value resembles a tag name (e.g., an error location like `version` or a path containing brackets).

## Options Considered

### Option 1: Spectre.Console + Spectre.Console.Cli (Chosen)

**Description:** Use Spectre.Console for all terminal rendering (tables, markup, colors) and Spectre.Console.Cli for command/argument parsing with strongly-typed settings classes.

**Pros:**
- Rich rendering: tables, trees, markup, colors, progress bars
- Strongly-typed command settings with validation
- Built-in help generation
- Graceful fallback in non-interactive terminals (CI-friendly)
- Active maintenance, large .NET community adoption
- Single package family covers both rendering and command parsing

**Cons:**
- Adds a NuGet dependency (not part of the .NET SDK)
- Rendering customization has a learning curve
- Output tests require snapshot/string comparison rather than structured assertions

### Option 2: System.CommandLine

**Description:** Use Microsoft's System.CommandLine library for parsing, with manual Console.Write for output.

**Pros:**
- Microsoft-maintained, close to the .NET platform
- Good argument binding and middleware pipeline
- Built-in help and completion generation

**Cons:**
- Still not stable/GA after years of development — API surface has changed repeatedly
- No built-in rendering capabilities — would need separate library or manual formatting for tables and colored output
- Two separate concerns (parsing + rendering) solved by two different approaches

### Option 3: Raw args parsing + manual output

**Description:** Parse `args[]` manually or with a minimal library, format all output with `Console.Write`.

**Pros:**
- Zero dependencies
- Full control over parsing and output
- Simplest possible dependency graph

**Cons:**
- Significant boilerplate for six commands with varying arguments
- No built-in help generation
- Manual table formatting is error-prone and tedious
- No color/markup without reinventing Spectre's functionality

## Rationale

Spectre.Console.Cli provides strongly-typed command definitions that map cleanly to the six ARS commands, each with their own settings class. This eliminates manual argument parsing boilerplate. Spectre.Console's rendering (tables, color-coded severity, grouped output) directly supports the human-readable reporting requirements (PRD §12.2, §17.1) without custom formatting code.

System.CommandLine was rejected due to its long-running beta status and lack of built-in rendering. Raw parsing was rejected because the overhead of implementing argument validation, help generation, and table formatting from scratch is disproportionate for a tool with six commands and rich output requirements.

## Consequences

### Positive

- Command definitions are declarative and self-documenting
- Tables, colors, and structured output are available immediately
- Help text is generated automatically from command/settings definitions
- CI-friendly: Spectre detects non-interactive terminals and degrades gracefully

### Negative

- External NuGet dependency that must be kept updated
- Output formatting is coupled to Spectre's rendering model — switching frameworks later would require rewriting reporters- Dynamic string values (error locations, user-supplied paths) must be bracket-escaped or rendered through non-markup APIs to prevent Spectre’s tag parser from throwing on content that resembles a color or style name- Snapshot testing for console output is more brittle than structured assertions

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | Defines the CLI framework constraint (§13.2) |
| [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) | Uses Spectre.Console.Cli for command routing |
| [ADR-0001 — Use .NET for CLI](ADR-0001-use-dotnet-for-cli.md) | Runtime prerequisite for Spectre.Console |
