# ADR-0001: Use .NET for a Cross-Platform CLI

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-28 |
| **Owner(s) / Deciders** | — |
| **Related RFC** | [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |

---

## Decision

Use .NET with C# as the runtime and language for Advanced Repo Spine v1.

## Context

Advanced Repo Spine is a CLI tool that must run on Windows, Linux, and macOS (PRD-0001 §13.1). It performs JSON parsing, filesystem traversal, tree comparison, and structured output formatting. The tool must be deterministic, fast to start, and produce both human-readable and machine-readable output.

The runtime choice affects the entire development experience: available libraries, build/publish workflow, cross-platform behavior, and contributor accessibility.

## Options Considered

### Option 1: .NET with C# (Chosen)

**Description:** Build the CLI as a .NET console application using C# and the standard .NET SDK. Publish as a self-contained executable or framework-dependent app.

**Pros:**
- Mature cross-platform support via .NET 8+ (Windows, Linux, macOS)
- Strong type system and pattern matching suited to tree comparison logic
- System.Text.Json built into the runtime — no external JSON dependency
- Spectre.Console ecosystem provides rich CLI rendering and command parsing
- Single-file publish produces a standalone executable
- Strong IDE support (Visual Studio, Rider, VS Code with C# Dev Kit)

**Cons:**
- Self-contained executables are larger (~60-80MB) compared to Go or Rust binaries
- .NET SDK required for development (not pre-installed on most systems)
- Startup time slightly higher than native binaries (mitigated by AOT in .NET 8+)

### Option 2: Go

**Description:** Build the CLI in Go, producing native binaries for each platform.

**Pros:**
- Small, statically-linked native binaries
- Fast startup
- Strong cross-compilation support
- No runtime dependency on target machine
- Good standard library for filesystem and JSON

**Cons:**
- Less expressive type system (no generics until recently, no pattern matching, no discriminated unions)
- CLI framework ecosystem less mature than Spectre.Console for rich output formatting
- Error handling via return values rather than exceptions adds verbosity for a tool with many failure paths

### Option 3: Rust

**Description:** Build the CLI in Rust, producing native binaries.

**Pros:**
- Fastest possible binaries
- Strong type system with enums, pattern matching, and algebraic data types
- Small binary size
- No runtime dependency

**Cons:**
- Steeper learning curve limits contributor accessibility
- Longer compile times during development
- CLI framework ecosystem (clap) is solid but less polished for rich rendering than Spectre.Console
- Serialization ecosystem (serde) is excellent but requires proc macros for derive, adding compile complexity

## Rationale

.NET with C# provides the best balance of developer productivity, type system expressiveness, cross-platform support, and ecosystem quality for this tool. The built-in System.Text.Json eliminates external dependencies for the core serialization need. Spectre.Console (chosen in ADR-0002) provides the richest CLI rendering ecosystem available, and it requires .NET.

The primary trade-off — larger binary size and slightly slower startup — is acceptable for a CLI tool that parses files and produces reports. Users run it occasionally, not in tight loops. Native AOT compilation in .NET 8+ mitigates startup concerns if needed.

Go and Rust produce smaller, faster binaries but offer less expressive type systems for the tree manipulation and pattern matching central to the comparison engine.

## Consequences

### Positive

- Access to Spectre.Console ecosystem for CLI rendering
- System.Text.Json available without additional packages
- Strong refactoring support in IDEs
- Familiar to a large developer community
- Single-file publish simplifies distribution

### Negative

- Contributors need the .NET SDK installed
- Self-contained binaries are larger than Go/Rust equivalents
- Framework-dependent deployment requires .NET runtime on target machine (unless self-contained)

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | Defines the runtime constraint (§13.1) |
| [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) | Design built on this runtime choice |
| [ADR-0002 — Use Spectre.Console](ADR-0002-use-spectre-console.md) | CLI framework choice dependent on .NET |
