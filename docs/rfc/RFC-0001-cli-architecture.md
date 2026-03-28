# RFC-0001 — CLI Architecture

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Owner(s)** | — |
| **Reviewers** | — |
| **Date** | 2026-03-28 |
| **Related PRD** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-ars-v1.md) |
| **Related Links** | [ADR-0001](../adr/ADR-0001-use-dotnet-for-cli.md), [ADR-0002](../adr/ADR-0002-use-spectre-console.md), [ADR-0003](../adr/ADR-0003-use-json-model-format.md), [ADR-0004](../adr/ADR-0004-v1-read-only.md), [ADR-0005](../adr/ADR-0005-v1-model-schema.md), [ADR-0006](../adr/ADR-0006-comparison-semantics.md), [ADR-0007](../adr/ADR-0007-report-export-aliases.md), [ADR-0008](../adr/ADR-0008-default-model-filename.md) |

---

## Summary

This RFC proposes the CLI architecture for Advanced Repo Spine v1: a .NET console application built with Spectre.Console.Cli, structured into five distinct layers (CLI, model parsing, filesystem scanning, comparison engine, reporting). The application exposes six commands (`init`, `validate`, `compare`, `report`, `suggest`, `export`) that operate on a JSON repository structure model. The key trade-off is choosing a layered, interface-driven architecture over a simpler monolithic approach — this adds upfront abstraction cost but keeps the comparison engine independently testable and enables future extensibility without v1 over-engineering.

---

## Context / Background

PRD-0001 defines Advanced Repo Spine as a cross-platform .NET CLI that makes repository structure explicit, inspectable, and machine-usable. The PRD specifies six commands, a JSON model format, deterministic output, and strict read-only behavior (except `init`). It recommends separating concerns across CLI, model parsing, filesystem scanning, comparison, and reporting layers (PRD §21).

No code exists yet. This RFC is the first design document and proposes how to structure the codebase, how commands flow through the system, what interfaces connect the layers, and how the JSON model and comparison results are represented in memory.

The technical constraints are fixed: .NET with C#, Spectre.Console + Spectre.Console.Cli, System.Text.Json, cross-platform on Windows/Linux/macOS (PRD §13).

---

## Problem Statement

The PRD defines *what* the CLI must do but does not specify *how* the components should be organized, how data flows between them, or what interfaces connect each layer. Without a design, an implementer would need to make significant unguided decisions about:

- project structure and assembly organization
- how the JSON model maps to in-memory types
- how filesystem scanning produces a comparable structure
- how the comparison engine matches expected vs. actual items
- how results flow from comparison to human and machine output
- how commands share infrastructure without coupling

---

## Goals

1. **Define the project structure** — solution layout, projects, namespaces (PRD §21)
2. **Define the data flow** — how each command loads input, processes it, and produces output (PRD §10)
3. **Specify the JSON model's in-memory representation** — types for the structure model (PRD §14, §15)
4. **Specify the comparison result types** — how findings are represented (PRD §16)
5. **Specify key interfaces** — contracts between layers so they can be tested and evolved independently (PRD §20.5)
6. **Define the CLI command surface** — how Spectre.Console.Cli maps to domain operations (PRD §11)
7. **Keep the design minimal** — do not over-engineer for v2 scenarios (PRD §27)

---

## Non-Goals

- Defining internal algorithm details for comparison matching (that is an implementation concern)
- Defining the exact content of the starter model produced by `init` (deferred to implementation)
- Designing a plugin architecture or extensibility surface (PRD §6 non-goal)
- Designing YAML support, IDE extensions, watch mode, or multi-repo features (PRD §6 non-goals)
- Prescribing internal class hierarchies, private methods, or variable naming conventions

---

## Requirements

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| R-1 | Six CLI commands: `init`, `validate`, `compare`, `report`, `suggest`, `export` | Must | PRD §10, §11 |
| R-2 | JSON model loaded and deserialized into typed objects | Must | PRD §14, §15 |
| R-3 | Filesystem scanning produces a normalized structure representation | Must | PRD §10.3 |
| R-4 | Comparison engine operates on model + scan result, produces typed findings | Must | PRD §16, §20.4 |
| R-5 | Comparison engine is independently testable without CLI or filesystem | Must | PRD §20.4, §20.5 |
| R-6 | Human-readable console output via Spectre.Console | Must | PRD §12.2 |
| R-7 | Machine-readable JSON output via System.Text.Json | Must | PRD §12.3, §17.2 |
| R-8 | Deterministic output ordering for same input | Must | PRD §20.1 |
| R-9 | Non-zero exit codes on failure | Must | PRD §18 |
| R-10 | Cross-platform path normalization | Must | PRD §13.4 |
| R-11 | Ignore rules respected during scanning and comparison | Must | PRD §10.3, §15.2 |
| R-12 | Model validation covers JSON syntax, required fields, field types, semantic constraints | Must | PRD §10.2 |
| R-13 | `init` must not overwrite existing files unless forced | Must | PRD §10.1, §19.1 |
| R-14 | Suggestion engine matches paths against model and explains reasoning | Should | PRD §10.5 |

---

## Constraints / Invariants

### Constraints

- **Runtime:** .NET 8+ (current LTS), C# (PRD §13.1)
- **CLI framework:** Spectre.Console for rendering, Spectre.Console.Cli for command routing (PRD §13.2)
- **Serialization:** System.Text.Json only — no Newtonsoft.Json, no YAML libraries (PRD §13.3)
- **Read-only:** No filesystem writes except the `init` command writing a new model file (PRD §20.2)
- **Single model file:** v1 loads exactly one JSON model file (no merging)

### Invariants

- The same model file + same directory state must always produce the same comparison output in the same order (determinism, PRD §20.1)
- All paths in the model and in scan results use forward-slash (`/`) as the normalized separator, regardless of host OS
- Comparison findings never mutate the model or the filesystem
- Exit code `0` means no blocking issues; any non-zero exit code means failure or structural issues detected (PRD §18)

---

## Proposed Design

### Project Structure

A single .NET solution with two projects:

```
advanced-ars/
├── src/
│   └── Ars.Cli/                     # Console application (entry point + commands)
│       ├── Ars.Cli.csproj
│       ├── Program.cs               # Entry point, Spectre app configuration
│       ├── Commands/                 # Spectre.Console.Cli command classes
│       │   ├── InitCommand.cs
│       │   ├── ValidateCommand.cs
│       │   ├── CompareCommand.cs
│       │   ├── ReportCommand.cs
│       │   ├── SuggestCommand.cs
│       │   └── ExportCommand.cs
│       ├── Infrastructure/          # CLI-specific infrastructure
│       │   └── ExitCodes.cs
│       ├── Model/                   # JSON model types + parsing
│       │   ├── RepoModel.cs
│       │   ├── ModelItem.cs
│       │   ├── ModelRules.cs
│       │   ├── ModelLoader.cs
│       │   └── ModelValidator.cs
│       ├── Scanning/                # Filesystem scanning
│       │   ├── FileSystemScanner.cs
│       │   └── ScannedItem.cs
│       ├── Comparison/              # Comparison engine
│       │   ├── ComparisonEngine.cs
│       │   ├── ComparisonResult.cs
│       │   └── Finding.cs
│       ├── Suggestion/              # Path suggestion engine
│       │   └── SuggestionEngine.cs
│       └── Reporting/               # Output formatting
│           ├── ConsoleReporter.cs
│           └── JsonExporter.cs
└── tests/
    └── Ars.Cli.Tests/               # Unit and integration tests
        ├── Ars.Cli.Tests.csproj
        ├── Model/
        ├── Comparison/
        ├── Scanning/
        ├── Suggestion/
        └── Commands/
```

**Why a single project (not a multi-project library split):** v1 has one consumer — the CLI itself. Splitting into `Ars.Core` + `Ars.Cli` adds project references, packaging concerns, and versioning complexity for zero current benefit. The internal namespace separation achieves the same logical boundaries. If a library is needed later (e.g., for an IDE extension), the namespaces are already clean enough to extract.

### Data Flow

Each command follows a consistent pipeline:

```
User Input (CLI args)
  → Command class (validates args, loads resources)
    → Domain operation (model loading, scanning, comparison, suggestion)
      → Result object (typed, serializable)
        → Reporter/Exporter (formats output)
          → Console/stdout + exit code
```

#### Command-specific flows:

**`init`**
```
CLI args (--path, --force)
  → Generate default RepoModel in memory
    → Serialize to JSON
      → Write to disk (abort if file exists and --force not set)
        → Console confirmation + exit 0
```

**`validate`**
```
CLI args (--model)
  → ModelLoader.Load(path)  [parse JSON, deserialize]
    → ModelValidator.Validate(model)  [semantic checks]
      → List<ValidationError> or success
        → Console report + exit code (0 = valid, non-zero = invalid)
```

**`compare`**
```
CLI args (--model, --root)
  → ModelLoader.Load(path)
    → ModelValidator.Validate(model)  [fail fast if invalid]
      → FileSystemScanner.Scan(root, model.Rules)
        → ComparisonEngine.Compare(model, scanResult)
          → ComparisonResult
            → ConsoleReporter.Render(result) or JsonExporter.Export(result)
              → exit code (0 = no issues, non-zero = issues found)
```

**`report`** — Same as `compare` but defaults to human-readable focused output. Accepts `--format text|json`.

**`suggest`**
```
CLI args (path-or-hint, --model)
  → ModelLoader.Load(path)
    → SuggestionEngine.Suggest(model, pathOrHint)
      → SuggestionResult (matched locations + reasoning)
        → Console output + exit code
```

**`export`** — Same as `compare` but defaults to JSON output and excludes console formatting.

### Resolving `compare` / `report` / `export` Overlap

PRD §11 notes that `compare`, `report`, and `export` may overlap and recommends resolving this via an ADR. The proposed approach:

- **`compare`** is the core analysis command. It performs the full pipeline (load → validate → scan → compare) and outputs results. It accepts `--format text|json` to control output format. Default format: `text`.
- **`report`** is an alias for `compare --format text`. It exists for ergonomic clarity — users invoking `ars report` communicate intent to read output, not pipe it. Internally it delegates to the same code path.
- **`export`** is an alias for `compare --format json`. It exists for ergonomic clarity — users invoking `ars export` communicate intent to consume output programmatically.

All three commands share the same comparison pipeline. The only difference is the default output format. This avoids code duplication and keeps behavior consistent.

**This design decision should be recorded as an ADR.**

### CLI Command Design (Spectre.Console.Cli)

Entry point configures the Spectre `CommandApp`:

```
Program.cs:
  var app = new CommandApp();
  app.Configure(config => {
      config.AddCommand<InitCommand>("init");
      config.AddCommand<ValidateCommand>("validate");
      config.AddCommand<CompareCommand>("compare");
      config.AddCommand<ReportCommand>("report");
      config.AddCommand<SuggestCommand>("suggest");
      config.AddCommand<ExportCommand>("export");
  });
  return app.Run(args);
```

Each command class inherits from `Command<TSettings>` where `TSettings` defines the CLI arguments and options for that command.

#### Common Settings

```
--model <path>     Path to the JSON model file (default: ars.json)
```

#### Per-Command Settings

| Command | Arguments / Options |
|---------|-------------------|
| `init` | `--path <path>` (output file, default: `ars.json`), `--force` (overwrite existing) |
| `validate` | `--model <path>` |
| `compare` | `--model <path>`, `--root <path>` (target directory, default: `.`), `--format text\|json` |
| `report` | `--model <path>`, `--root <path>` |
| `suggest` | `<path>` (positional: the path to suggest placement for), `--model <path>` |
| `export` | `--model <path>`, `--root <path>` |

### Model Layer

The JSON model (PRD §14, §15) deserializes into these types:

#### `RepoModel` (root)

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Version` | `string` | Yes | Schema version (`"1.0"`) |
| `Name` | `string` | Yes | Display name |
| `Description` | `string?` | No | Explanatory text |
| `Rules` | `ModelRules` | Yes | Global comparison rules |
| `Items` | `List<ModelItem>` | Yes | Top-level expected structure |

#### `ModelRules`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `CaseSensitive` | `bool` | Yes | Whether path matching is case-sensitive |
| `Ignore` | `List<string>` | No | Paths or patterns to ignore during scanning |

#### `ModelItem`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Name` | `string` | Yes | Human-friendly node name |
| `Type` | `string` | Yes | `"directory"` or `"file"` |
| `Path` | `string` | Yes | Expected repository-relative path (forward-slash normalized) |
| `Required` | `bool` | Yes | Whether absence is an error |
| `Description` | `string?` | No | Guidance text |
| `Children` | `List<ModelItem>?` | No | Nested items (directories only) |

System.Text.Json deserialization uses `JsonPropertyNameAttribute` with camelCase naming to match the JSON format defined in PRD §14.3.

#### Model Validation Rules

`ModelValidator` checks:

1. `Version` is present and equals `"1.0"`
2. `Name` is present and non-empty
3. `Rules` is present
4. `Items` is present and non-empty
5. Every item has `Name`, `Type`, `Path`
6. `Type` is one of `"directory"` or `"file"`
7. `Path` values are unique across the entire model (flattened)
8. `Path` values use forward-slash only (no backslash)
9. `Children` are only present on items with `Type = "directory"`
10. No circular references in the hierarchy
11. Child `Path` values are prefixed by parent `Path` (structural consistency)

Validation produces a `List<ValidationError>`, each with a location description and message.

### Scanning Layer

`FileSystemScanner` traverses a directory tree and produces a flat list of `ScannedItem` entries.

#### `ScannedItem`

| Field | Type | Description |
|-------|------|-------------|
| `Path` | `string` | Repository-relative path, forward-slash normalized |
| `Type` | `string` | `"directory"` or `"file"` |

#### Scanning Behavior

1. Accept the root directory path and `ModelRules` as input
2. Recursively enumerate all files and directories
3. Normalize all paths to forward-slash, relative to the root
4. Apply ignore rules: skip any path matching an ignore pattern. Ignore matching uses prefix comparison for directory patterns (ending in `/`) and exact match for others. Wildcard matching (glob) is deferred to a future version.
5. Sort results in deterministic order (ordinal string comparison on normalized path)
6. Return `List<ScannedItem>`

The scanner does not depend on the model structure — it produces a raw inventory of what exists on disk.

### Comparison Layer

`ComparisonEngine` takes a `RepoModel` and a `List<ScannedItem>` and produces a `ComparisonResult`.

#### `Finding`

| Field | Type | Description |
|-------|------|-------------|
| `Type` | `FindingType` | `Missing`, `Present`, `OptionalMissing`, `Unmatched`, `Misplaced` |
| `Severity` | `FindingSeverity` | `Error`, `Warning`, `Info` |
| `ExpectedPath` | `string?` | Path from the model (null for unmatched items) |
| `ActualPath` | `string?` | Path from the scan (null for missing items) |
| `ItemName` | `string?` | Model item name, if applicable |
| `Message` | `string` | Human-readable explanation |

#### `FindingType` → Severity mapping

| FindingType | Severity | Condition |
|-------------|----------|-----------|
| `Missing` | `Error` | Model item with `Required = true` not found in scan |
| `Present` | `Info` | Model item found in scan at expected path |
| `OptionalMissing` | `Info` | Model item with `Required = false` not found in scan |
| `Unmatched` | `Warning` | Scanned item not matched to any model item |
| `Misplaced` | `Warning` | Scanned item appears to match a model item but at a different path |

#### `ComparisonResult`

| Field | Type | Description |
|-------|------|-------------|
| `ModelVersion` | `string` | From the model |
| `ModelName` | `string` | From the model |
| `Root` | `string` | Scanned root directory |
| `Timestamp` | `DateTimeOffset` | When comparison was run |
| `Settings` | `ComparisonSettings` | Normalized settings used |
| `Summary` | `ComparisonSummary` | Counts by finding type |
| `Findings` | `List<Finding>` | All findings, deterministically ordered |

#### `ComparisonSummary`

| Field | Type |
|-------|------|
| `Missing` | `int` |
| `Present` | `int` |
| `OptionalMissing` | `int` |
| `Unmatched` | `int` |
| `Misplaced` | `int` |
| `Total` | `int` |

#### Comparison Algorithm (High-Level)

1. Flatten the model hierarchy into a list of all expected items with their full paths
2. Build a lookup from normalized path → model item
3. For each model item:
   - Search the scan results for a matching path (respecting case-sensitivity setting)
   - If found: emit `Present`
   - If not found and `Required = true`: emit `Missing`
   - If not found and `Required = false`: emit `OptionalMissing`
4. For each scanned item not matched in step 3:
   - Attempt conservative misplacement detection: check if the item's filename matches any unresolved missing model item's filename. If exactly one match exists, emit `Misplaced`. Otherwise, emit `Unmatched`.
5. Sort all findings by: type ordinal, then path (ordinal string comparison)

The misplacement detection is deliberately conservative (PRD §16.5, §22.2). It only claims misplacement when there is a single unambiguous filename match against a missing expected item. All other unmatched items are classified as `Unmatched`.

### Suggestion Layer

`SuggestionEngine` takes a `RepoModel` and a path (or path-like hint) and returns the best matching locations from the model.

#### Suggestion Algorithm (High-Level)

1. Parse the input path/hint into components (directory parts, filename, extension)
2. Search the model for items whose path or name partially matches the input
3. Rank matches by specificity: exact path match > parent directory match > filename pattern match > name similarity
4. Return the top suggestions with explanation text
5. If no match is found, return an explicit "no suggestion available" result

#### `SuggestionResult`

| Field | Type | Description |
|-------|------|-------------|
| `Input` | `string` | The original path/hint |
| `Suggestions` | `List<Suggestion>` | Ranked suggestions (may be empty) |
| `Message` | `string` | Summary explanation |

#### `Suggestion`

| Field | Type | Description |
|-------|------|-------------|
| `Path` | `string` | Suggested model path |
| `Reason` | `string` | Why this was suggested |
| `Confidence` | `string` | `"high"`, `"medium"`, `"low"` |

### Reporting Layer

Two output formatters that consume `ComparisonResult`:

#### `ConsoleReporter`

Uses Spectre.Console to render:

1. **Header:** Model name, root path, timestamp
2. **Summary table:** Counts per finding type, color-coded (red for errors, yellow for warnings, green for present)
3. **Findings by category:** Grouped sections for Missing, Misplaced, Unmatched, Optional Missing — each showing path, expected path, and message
4. **Footer:** Total findings count and result (pass/fail)

#### `JsonExporter`

Serializes `ComparisonResult` directly to JSON using System.Text.Json with:
- camelCase property naming
- Indented formatting
- Enum values serialized as strings
- Null values omitted
- Output written to stdout

The JSON output shape matches PRD §17.2 example.

### Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success — command completed, no blocking issues |
| `1` | Structural issues detected (comparison found missing/misplaced items) |
| `2` | Invalid input (bad arguments, file not found, invalid JSON) |
| `3` | Internal error (unexpected exception) |

---

## Data Model / Interfaces / Contracts

### JSON Model Schema (Input)

The input JSON model follows the shape defined in PRD §14.3:

```json
{
  "version": "1.0",
  "name": "string (required)",
  "description": "string (optional)",
  "rules": {
    "caseSensitive": false,
    "ignore": ["string"]
  },
  "items": [
    {
      "name": "string (required)",
      "type": "directory | file",
      "path": "string (required, forward-slash, repo-relative)",
      "required": true,
      "description": "string (optional)",
      "children": []
    }
  ]
}
```

### JSON Comparison Output Schema (Export)

```json
{
  "modelVersion": "1.0",
  "modelName": "string",
  "root": "string",
  "timestamp": "ISO-8601",
  "settings": {
    "caseSensitive": false,
    "ignoredPatterns": ["string"]
  },
  "summary": {
    "missing": 0,
    "present": 0,
    "optionalMissing": 0,
    "unmatched": 0,
    "misplaced": 0,
    "total": 0
  },
  "findings": [
    {
      "type": "missing | present | optionalMissing | unmatched | misplaced",
      "severity": "error | warning | info",
      "expectedPath": "string | null",
      "actualPath": "string | null",
      "itemName": "string | null",
      "message": "string"
    }
  ]
}
```

### Key Interfaces (Conceptual Contracts)

These are the contracts between layers. Implementation may use interfaces, abstract classes, or concrete classes — the contract is the behavior.

**Model Loading:**
- Input: file path (string)
- Output: `RepoModel` or throws with descriptive error
- Behavior: reads file, parses JSON, deserializes to typed model

**Model Validation:**
- Input: `RepoModel`
- Output: `List<ValidationError>` (empty = valid)
- Behavior: checks all semantic rules, returns all errors (not just the first)

**Filesystem Scanning:**
- Input: root directory path (string), `ModelRules`
- Output: `List<ScannedItem>`, sorted deterministically
- Behavior: recursive traversal, path normalization, ignore filtering

**Comparison:**
- Input: `RepoModel`, `List<ScannedItem>`
- Output: `ComparisonResult`
- Behavior: matching, finding classification, deterministic ordering
- **Critical:** No filesystem access. No CLI dependency. Operates purely on in-memory data.

**Suggestion:**
- Input: `RepoModel`, path/hint (string)
- Output: `SuggestionResult`
- Behavior: matching, ranking, explanation generation

**Reporting:**
- Input: `ComparisonResult`
- Output: formatted text to console (ConsoleReporter) or JSON string (JsonExporter)

---

## Alternatives Considered

### Alternative 1: Multi-Project Library Split

**Description:** Separate the solution into `Ars.Core` (library with model, comparison, scanning) and `Ars.Cli` (console app with commands and reporting). The library could be published as a NuGet package.

**Strengths:**
- Clean binary separation between domain logic and CLI
- Library reusable by other consumers (IDE extensions, CI tools) without taking a CLI dependency
- Standard .NET convention for tools with reusable logic

**Why not selected:** v1 has exactly one consumer — the CLI. Adding a separate project introduces packaging, versioning, and project-reference overhead for no current benefit. The namespace separation in the single-project structure provides the same logical boundaries. Extracting a library later is straightforward because the key invariant (comparison engine has no CLI/filesystem dependencies) is enforced by interface design, not project boundaries. This aligns with PRD §27: "Keep v1 narrowly focused and finishable."

### Alternative 2: Minimal Flat Structure (No Namespace Layering)

**Description:** A single project with all types in a flat structure or minimal namespaces. Commands, model types, comparison logic, and reporting all in the root namespace or a few broad groupings.

**Strengths:**
- Simplest possible structure
- Fastest to get started
- No decisions about where things go

**Why not selected:** Flat structure makes it nearly impossible to enforce the critical invariant that the comparison engine has no dependency on CLI or filesystem. It also makes testing harder — you cannot easily test comparison logic in isolation. For a project with five distinct concerns (CLI, model, scanning, comparison, reporting), minimal namespacing makes the code harder to navigate as it grows.

---

## Trade-Offs

| Gain | Cost |
|------|------|
| Comparison engine is independently testable (in-memory only) | Requires discipline to keep filesystem access out of comparison code |
| Namespace separation makes code navigable | Slightly more files and directories than a flat structure |
| `report`/`export` as aliases avoids code duplication | Users may be confused about why three commands exist — needs clear help text |
| Single project keeps build simple | If a library is needed later, extraction requires moving files |
| Conservative misplacement detection avoids false positives | Some genuinely misplaced items may be classified as "unmatched" |
| Forward-slash normalization simplifies comparison | Path display on Windows shows `/` instead of native `\` |
| Deterministic output via ordinal sorting | Output order may not match intuitive groupings (e.g., parent before children) |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Model schema proves too limited for real-world repos | Medium | Medium | Keep schema extensible (unknown fields ignored); v2 can add fields without breaking v1 models |
| Ignore rule matching without glob support frustrates users | Medium | Low | Prefix matching covers the most common case (`.git/`, `bin/`, `obj/`); glob support is a clear v2 candidate |
| Misplacement detection produces confusing results | Low | Medium | Conservative by design — only claims misplacement on single unambiguous filename match; all other cases are "unmatched" |
| Spectre.Console output differences across terminals | Low | Low | Stick to tables, trees, and basic markup; avoid terminal-specific features |
| Large repositories produce slow scans | Low | Medium | v1 targets typical project repos (<10K files); performance optimization deferred unless needed |

---

## Testing / Validation Strategy

### Unit Tests

**Model parsing:**
- Valid JSON deserializes correctly
- Missing required fields produce descriptive errors
- Invalid field values (wrong type, empty path, backslash in path) are caught
- All validation rules listed in Model Validation Rules section are individually tested

**Comparison engine:**
- Missing required item → `Finding.Type = Missing, Severity = Error`
- Present required item → `Finding.Type = Present, Severity = Info`
- Absent optional item → `Finding.Type = OptionalMissing, Severity = Info`
- Unmatched scanned item → `Finding.Type = Unmatched, Severity = Warning`
- Misplaced item (single filename match) → `Finding.Type = Misplaced, Severity = Warning`
- Ambiguous filename match → `Finding.Type = Unmatched` (conservative)
- Case-sensitive vs. case-insensitive matching
- Deterministic output ordering (same input → same output)
- Empty model items → valid comparison with all scanned items unmatched
- Empty scan results → all required items missing

**Suggestion engine:**
- Exact path match → high confidence
- Parent directory match → medium confidence
- No match → explicit "no suggestion" result

**These tests operate entirely in memory — no filesystem access, no CLI invocation.**

### Integration Tests

**Filesystem scanning:**
- Create a temporary directory structure, scan it, verify normalized paths
- Verify ignore rules exclude specified paths
- Verify cross-platform path normalization (forward slashes on all OSes)
- Verify deterministic ordering

**End-to-end command tests:**
- Run each command via `CommandApp.Run()` with in-process invocation
- Verify exit codes match expected values
- Verify JSON output is valid and matches schema
- Verify `init` creates a valid model file
- Verify `validate` accepts the `init` output
- Verify `compare` produces expected findings for a known directory + model

### Acceptance Tests (Mapped to PRD §19)

| PRD Criterion | Test |
|---------------|------|
| §19.1 `init` generates valid model | `init` → `validate` → exit 0 |
| §19.1 No accidental overwrite | `init` on existing file without `--force` → exit 2 |
| §19.2 Invalid JSON rejected | `validate` on malformed JSON → exit 2, descriptive error |
| §19.2 Invalid semantics rejected | `validate` on model with duplicate paths → exit 2 |
| §19.3 Missing required items detected | `compare` on repo missing a required item → finding with type `missing` |
| §19.3 Unmatched items detected | `compare` on repo with extra files → finding with type `unmatched` |
| §19.3 Ignore rules respected | `compare` ignores `.git/` when in ignore list |
| §19.3 Deterministic output | Same inputs → same output bytes |
| §19.5 Suggestion works | `suggest` with known path → suggestion with explanation |
| §19.5 No match handled | `suggest` with unknown path → "no suggestion" message |
| §19.6 JSON export stable | `export` produces valid JSON matching schema |
| §19.7 Cross-platform | Tests run on Windows and Linux CI |

---

## Open Questions (Resolved)

- [x] **Default model file name:** Use `ars.json` (visible file). PRD §11 examples consistently use `ars.json`, and a visible file is more discoverable for both humans and AI agents. Hidden files (`.ars.json`) add confusion on Windows where dotfile conventions are weaker. Recorded in ADR-0008.
- [x] **Ignore pattern syntax:** v1 uses prefix matching for directories (patterns ending in `/`) and exact match for files. Glob patterns (e.g., `*.tmp`) are deferred to v2. Prefix + exact match covers the primary use cases (`.git/`, `bin/`, `obj/`, `node_modules/`) without introducing pattern-matching complexity. See PRD §26.3, §26.4.
- [x] **Case sensitivity default:** Default to `false` (case-insensitive). This is the safer cross-platform default — it prevents false positives on case-insensitive filesystems (Windows, macOS default). Users on case-sensitive Linux filesystems can set `caseSensitive: true` in their model's `rules`. See PRD §26.5.
- [x] **Node IDs:** Deferred to v2. In v1, paths serve as unique identifiers and the validation rule requiring unique `path` values across the model enforces this. Adding `id` fields would increase model authoring burden for no v1 benefit. See PRD §26.6.
- [x] **`init` template content:** Produce a minimal valid model with 1–2 example items using `description` fields as inline documentation (compensating for JSON's lack of comments). The template should include a `README.md` file item and a `src/` directory item as recognizable examples. See PRD §26.7.

---

## Decision Outcome / Next Steps

The following ADRs should be created based on this RFC and PRD §24:

| ADR | Decision | Status |
|-----|----------|--------|
| [ADR-0001](../adr/ADR-0001-use-dotnet-for-cli.md) | Use .NET for a cross-platform CLI | Accepted |
| [ADR-0002](../adr/ADR-0002-use-spectre-console.md) | Use Spectre.Console and Spectre.Console.Cli | Accepted |
| [ADR-0003](../adr/ADR-0003-use-json-model-format.md) | Use JSON as the v1 model format | Accepted |
| [ADR-0004](../adr/ADR-0004-v1-read-only.md) | Keep v1 read-only except for `init` | Accepted |
| [ADR-0005](../adr/ADR-0005-v1-model-schema.md) | Define the v1 repository structure model schema | Accepted |
| [ADR-0006](../adr/ADR-0006-comparison-semantics.md) | Define comparison semantics (finding types and severity mapping) | Accepted |
| [ADR-0007](../adr/ADR-0007-report-export-aliases.md) | `report` and `export` as aliases for `compare --format` | Accepted |
| [ADR-0008](../adr/ADR-0008-default-model-filename.md) | Use `ars.json` as the default model file name | Accepted |

All ADRs have been accepted. Implementation can begin following the project structure and data flow defined in this RFC.

---

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-ars-v1.md) | Originating requirements document. This RFC proposes how to implement PRD-0001. |
