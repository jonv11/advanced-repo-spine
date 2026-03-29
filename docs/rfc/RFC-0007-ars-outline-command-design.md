# RFC-0007 — `ars outline` Command Design

| Field | Value |
|-------|-------|
| **Status** | Draft |
| **Target Release** | post-v1 |
| **Owner(s)** | — |
| **Reviewers** | — |
| **Date** | 2026-03-29 |
| **Related PRD** | [PRD-0002 — Repository Documentation Outline View](../prd/PRD-0002-repository-documentation-outline-view.md) |
| **Related Links** | [ADR-0009 — Add `ars outline` to the ARS Command Surface](../adr/ADR-0009-add-ars-outline-command.md), [ADR-0010 — ATX Headings Only in v1](../adr/ADR-0010-atx-headings-only-extraction.md), [ADR-0011 — Flat Heading List Under File Nodes](../adr/ADR-0011-flat-heading-list-under-file-nodes.md), [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md), [RFC-0003 — CLI Ergonomics and Discoverability Baseline](RFC-0003-cli-ergonomics.md) |

---

## Summary

This RFC specifies the full implementation design for `ars outline`, a new ARS discovery command that traverses a repository and produces a combined tree view of directories, files, and the ATX heading structure of each Markdown file. The command supports `--path`, `--max-depth`, `--headings-depth`, and `--format text|json` options. Heading extraction uses a line-by-line ATX regex with a code-fence guard — no external Markdown parser dependency. JSON output models the result as a flat, typed node tree (`directory`, `file`, `heading`). The key trade-off is ATX-only extraction (simpler, zero new dependencies) over full CommonMark parsing (complete but heavy).

---

## Context / Background

ARS v1, specified in PRD-0001 and implemented through RFC-0001 through RFC-0006, provides path-level repository understanding: `compare`, `report`, `suggest`, and `export` all operate on the model-vs-filesystem diff. None of these commands expose the internal structure of documentation.

PRD-0002 defines a post-v1 discovery gap: contributors and AI agents can locate `docs/` or `README.md` but cannot learn section layout without opening files. The PRD explicitly leaves parser details, rendering conventions, and internal architecture for this RFC.

This RFC operates within the existing CLI architecture (RFC-0001), must satisfy the help-output discoverability requirements of RFC-0003, and records three new architectural decisions as ADR-0009, ADR-0010, and ADR-0011.

---

## Problem Statement

The technical problem is: given an arbitrary repository path, produce a deterministic combined tree of filesystem nodes and Markdown heading nodes, with independently configurable depth limits for the filesystem dimension and the heading dimension, in both a human-readable terminal format and a machine-stable JSON format.

Four sub-problems require explicit design:

1. **Traversal**: how to walk the directory tree, apply `--max-depth`, and sort entries deterministically
2. **Heading extraction**: how to extract headings from `.md` files without a full CommonMark parser, including handling of code fences and malformed content
3. **Text rendering**: how to represent heading nodes in the Spectre.Console tree renderer alongside directory and file nodes
4. **JSON contract**: what schema shape satisfies FR-8's unified typed-node requirement while remaining simple to produce and consume

---

## Goals

| ID | Goal | Source |
|----|------|--------|
| G-1 | Implement `ars outline` as a Spectre.Console.Cli command registered in `Program.cs` | FR-1 |
| G-2 | Default traversal from CWD; scope to subtree via `--path` | FR-2 |
| G-3 | Inline `.md` heading expansion enabled by default with no extra flag | FR-3, FR-6 |
| G-4 | Preserve heading document order; expose level via `#`-prefix (text) and `level` field (JSON) | FR-4 |
| G-5 | Support `--max-depth <n>`, `--headings-depth <n>`, `--format text\|json` | FR-5 |
| G-6 | Deliver both `text` and `json` output from the first release | FR-7, FR-8 |
| G-7 | Handle headingless files, malformed Markdown, invalid paths without crashing | FR-9, FR-10, FR-11 |
| G-8 | Reuse ARS/model-native ignore behavior for traversal filtering | FR-12 |
| G-9 | Output is deterministic for identical repository state and options | NFR-1 |
| G-10 | Text output is terminal-readable; JSON field names are stable across releases | NFR-2, NFR-3 |
| G-11 | Snapshot/golden tests cover all acceptance criteria scenarios | NFR-5 |

## Non-Goals

- Full CommonMark parsing or setext heading extraction (see ADR-0010)
- `.gitignore` parsing or Git-aware ignore expansion
- `.mdx`, AsciiDoc, reStructuredText, or other non-`.md` document types
- Front matter extraction, anchor generation, line-number reporting, or link analysis
- Nested heading tree (H2 as child of H1) — flat list with `level` field is sufficient (see ADR-0011)
- Summary counts, documentation coverage scoring, or audit output
- Any repository mutation or auto-fix behavior

---

## Requirements

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| R-1 | Register `ars outline` as a new command in `Program.cs` | Must | PRD FR-1 |
| R-2 | Accept `--path <path>` option; default to CWD when omitted | Must | PRD FR-2 |
| R-3 | Traverse directory tree depth-first; expand `.md` files with ATX headings inline | Must | PRD FR-3 |
| R-4 | Preserve heading document order; emit `level` (1–6) per heading node | Must | PRD FR-4 |
| R-5 | Accept `--max-depth <n>` to limit filesystem depth; `--headings-depth <n>` to limit heading depth | Must | PRD FR-5 |
| R-6 | Enable heading expansion by default; require no extra activation flag | Must | PRD FR-6 |
| R-7 | Default `--format` is `text`; accept `text` or `json` | Must | PRD FR-7, FR-8 |
| R-8 | JSON output must use `type` discriminator with values `directory`, `file`, `heading` | Must | PRD FR-8 |
| R-9 | Files with no headings appear as file nodes with an empty children list | Must | PRD FR-9 |
| R-10 | Files with malformed or partially-parseable Markdown must not crash; file node is always emitted | Must | PRD FR-10 |
| R-11 | Invalid, unreadable, or out-of-scope `--path` exits non-zero with a clear error message | Must | PRD FR-11 |
| R-12 | Apply ARS/model-native ignore behavior to traversal (same files skipped as in `compare`) | Should | PRD FR-12 |
| R-13 | Text and JSON output ordering is byte-stable for identical repository state and options | Must | PRD NFR-1 |
| R-14 | Help output enumerates `--format text\|json`, states defaults, follows RFC-0003 conventions | Must | PRD NFR-2, RFC-0003 |
| R-15 | JSON field names are stable and presentation-independent | Must | PRD NFR-3 |
| R-16 | Heading extractor and traversal engine are independently unit-testable | Must | PRD NFR-5 |

---

## Constraints / Invariants

### Constraints

- ARS is a CLI-first, read-only product; `ars outline` must not write or modify any repository file
- No new NuGet package dependencies may be introduced for heading extraction (ATX regex covers the requirement)
- Output must be consistent across Windows, Linux, and macOS (path separator normalization applies)
- Help output must follow RFC-0003: option descriptions, accepted values enumerated, defaults visible

### Invariants

- The command must not alter the behavior or output of any existing command (`init`, `validate`, `compare`, `report`, `suggest`, `export`)
- Exit code 0 on success, non-zero on hard error (invalid path, unreadable root); partial-read degradation (permission on a single file) does not change the exit code to non-zero unless the root is unreadable

---

## Proposed Design

### Command Registration

Register `OutlineCommand` in `Program.cs` alongside the existing commands:

```csharp
app.AddCommand<OutlineCommand>("outline");
```

`OutlineCommand` inherits from `Command<OutlineCommand.Settings>` following the existing Spectre.Console.Cli pattern.

### Command Settings

```csharp
public class Settings : CommandSettings
{
    [CommandOption("--path <PATH>")]
    [Description("Root path to traverse. Defaults to the current working directory.")]
    public string? Path { get; set; }

    [CommandOption("--max-depth <N>")]
    [Description("Maximum filesystem depth to traverse. Omit for unlimited depth.")]
    public int? MaxDepth { get; set; }

    [CommandOption("--headings-depth <N>")]
    [Description("Maximum heading level to include (1–6). Omit for all heading levels.")]
    public int? HeadingsDepth { get; set; }

    [CommandOption("--format <FORMAT>")]
    [Description("Output format: text (default) or json.")]
    [DefaultValue("text")]
    public string Format { get; set; } = "text";
}
```

### Component Architecture

The command is split into three independently testable components following ARS's existing separation-of-concerns principle:

```
OutlineCommand
    └── OutlineScanner          — traversal + heading extraction → OutlineNode tree
            └── HeadingExtractor    — ATX heading parsing for a single file
    └── OutlineTextRenderer     — renders OutlineNode tree to terminal text
    └── OutlineJsonRenderer     — renders OutlineNode tree to JSON (System.Text.Json)
```

`OutlineScanner` and `HeadingExtractor` have no dependency on Spectre.Console and are testable in isolation.

### Traversal Algorithm (`OutlineScanner`)

1. Resolve `--path` to an absolute path using `Path.GetFullPath`. If the resolved path does not exist or is not readable, return a hard error (exit 1).
2. Determine traversal root: if `--path` is a file, emit a single file node (with headings) as the root. If it is a directory, begin recursive descent.
3. At each directory level:
   - Enumerate entries with `Directory.GetFileSystemEntries`
   - Sort entries: directories first (alphabetical), then files (alphabetical) within the same parent. This ordering is deterministic across platforms.
   - Recurse into each subdirectory unless current depth equals `--max-depth` (depth is counted from the root as 0)
   - For each `.md` file: call `HeadingExtractor.Extract(filePath, headingsDepth)` and attach the returned heading nodes as children
   - For non-`.md` files: emit a file node with empty children
4. Apply ARS/model-native ignore behavior: use the same ignore-filter logic already applied in `compare` scanning. If no ignore behavior exists yet, this requirement is a no-op for the first release.

**Depth counting:** root directory is depth 0; its direct children are depth 1. `--max-depth 1` shows the root and its direct children only. `--max-depth 0` shows the root node with no children.

### Heading Extraction (`HeadingExtractor`)

Extract ATX headings from a `.md` file using a line-by-line scan:

```
algorithm ExtractHeadings(filePath, maxLevel):
    headings = []
    inCodeFence = false
    fenceMarker = ""

    for each line in ReadAllLines(filePath):
        // Track code fences (``` or ~~~, with optional language tag)
        if line matches /^(`{3,}|~{3,})/:
            fenceToken = matched prefix
            if not inCodeFence:
                inCodeFence = true
                fenceMarker = fenceToken
            else if line starts with fenceMarker:
                inCodeFence = false
                fenceMarker = ""
            continue

        if inCodeFence:
            continue

        // Match ATX heading: 1–6 # chars, space, non-empty title
        if line matches /^(#{1,6})\s+(.+?)(\s+#+\s*)?$/:
            level = length of matched # group
            text = matched title group (trimmed)
            if maxLevel is null or level <= maxLevel:
                headings.append(HeadingNode { level, text })

    return headings
```

Key rules:
- Headings must start at column 0 (no leading spaces); CommonMark allows up to 3 spaces but this implementation requires column-0 for simplicity
- Trailing `#` characters (closing ATX syntax) are stripped
- An empty title after stripping produces no heading node
- If the file cannot be read (permission error), return an empty list and mark the file node with `error: true`; do not throw

### Data Model

```csharp
public record OutlineNode(
    OutlineNodeType Type,   // Directory | File | Heading
    string Name,            // directory/file name, or heading text
    string? Path,           // relative path from scope root (null for headings)
    int? Level,             // heading level 1–6 (null for directory/file)
    bool Error,             // true if node could not be fully read
    IReadOnlyList<OutlineNode> Children
);

public enum OutlineNodeType { Directory, File, Heading }
```

### Text Rendering (`OutlineTextRenderer`)

Use Spectre.Console's `Tree` widget. Heading nodes are added as children of their parent file node:

- **Directory node:** `[bold]{name}/[/]`
- **File node (non-`.md`):** `{name}`
- **File node (`.md`):** `{name}` — heading children appended below
- **Heading node:** `{hashes} {text}` where `hashes` is `#` repeated `level` times (e.g., `## Getting Started`)
- **Error indicator:** append ` [dim](unreadable)[/]` to a file node where `error: true`

Example output:

```
docs/
├── README.md
│   ├── # Introduction
│   ├── ## Installation
│   └── ## Usage
├── guides/
│   └── setup.md
│       └── # Setup Guide
└── CHANGELOG.md
    (no headings)
```

Files with no headings appear without heading children and without any placeholder text — the file node is shown as a leaf.

### JSON Rendering (`OutlineJsonRenderer`)

Serialize the `OutlineNode` tree using `System.Text.Json` with `JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }`.

JSON field mapping:

| C# field | JSON key | Present for |
|----------|----------|-------------|
| `Type` | `"type"` (lowercase string) | all nodes |
| `Name` | `"name"` | all nodes |
| `Path` | `"path"` | directory, file |
| `Level` | `"level"` | heading |
| `Error` | `"error"` | file (omit if false) |
| `Children` | `"children"` | directory, file (omit for heading) |

`OutlineNodeType` serializes as `"directory"`, `"file"`, or `"heading"` (lowercase).

Example:

```json
{
  "type": "directory",
  "name": "docs",
  "path": "docs",
  "children": [
    {
      "type": "file",
      "name": "README.md",
      "path": "docs/README.md",
      "children": [
        { "type": "heading", "level": 1, "name": "Introduction" },
        { "type": "heading", "level": 2, "name": "Installation" },
        { "type": "heading", "level": 2, "name": "Usage" }
      ]
    },
    {
      "type": "directory",
      "name": "guides",
      "path": "docs/guides",
      "children": [
        {
          "type": "file",
          "name": "setup.md",
          "path": "docs/guides/setup.md",
          "children": [
            { "type": "heading", "level": 1, "name": "Setup Guide" }
          ]
        }
      ]
    }
  ]
}
```

### Error Handling

| Condition | Behavior | Exit code |
|-----------|----------|-----------|
| `--path` does not exist | Print error, exit immediately | 1 |
| `--path` is not readable | Print error, exit immediately | 1 |
| Single file unreadable during traversal | Set `error: true` on file node, continue | 0 |
| Malformed Markdown | Emit valid headings found before the malformation, continue | 0 |
| Invalid `--format` value | Print error listing accepted values, exit | 1 |
| `--max-depth` or `--headings-depth` is negative | Print error, exit | 1 |

---

## Data Model / Interfaces / Contracts

### `OutlineNode` JSON Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "OutlineNode",
  "oneOf": [
    {
      "properties": {
        "type": { "const": "directory" },
        "name": { "type": "string" },
        "path": { "type": "string" },
        "children": { "type": "array", "items": { "$ref": "#" } }
      },
      "required": ["type", "name", "path", "children"]
    },
    {
      "properties": {
        "type": { "const": "file" },
        "name": { "type": "string" },
        "path": { "type": "string" },
        "error": { "type": "boolean" },
        "children": { "type": "array", "items": { "$ref": "#" } }
      },
      "required": ["type", "name", "path", "children"]
    },
    {
      "properties": {
        "type": { "const": "heading" },
        "name": { "type": "string" },
        "level": { "type": "integer", "minimum": 1, "maximum": 6 }
      },
      "required": ["type", "name", "level"]
    }
  ]
}
```

**Stability guarantee:** `type`, `name`, `path`, `level`, `children` field names are stable. The `error` field is optional and omitted when false. No additional fields will be added to heading nodes without a new ADR.

---

## Alternatives Considered

### Alternative 1: Extend `compare` or `report` with a `--headings` mode

**Description:** Add a flag to existing commands that enriches their output with heading data.

**Strengths:** Fewer top-level commands; heading data co-located with model-diff output.

**Why not selected:** `compare` and `report` operate on the model-vs-filesystem diff (ADR-0006). Embedding heading traversal there conflates two unrelated concerns — repository structure validation and documentation discovery. PRD-0002 §Out of Scope explicitly lists this. See ADR-0009 for the command-surface decision.

### Alternative 2: Full CommonMark parser (e.g., Markdig)

**Description:** Add Markdig (or equivalent) as a NuGet dependency and parse `.md` files into a full AST, extracting heading nodes from the AST.

**Strengths:** Handles all heading syntax including setext; handles edge cases like headings in HTML blocks; future-proof for additional extraction (links, front matter, etc.).

**Why not selected:** Adds a new runtime dependency requiring version management and license review. ATX headings cover the overwhelming majority of `.md` files in modern repositories. The code-fence guard is the only non-trivial rule needed. A full parser is disproportionate to the extraction goal. See ADR-0010 for the heading-extraction scope decision.

### Alternative 3: Single `--depth` parameter for both filesystem and heading depth

**Description:** One `--depth <n>` parameter that limits both directory recursion depth and heading level depth simultaneously.

**Strengths:** Simpler option surface; one concept to explain.

**Why not selected:** Filesystem depth and heading depth are genuinely independent dimensions. A large monorepo might need `--max-depth 2` to keep directory output manageable while still wanting all six heading levels. Conversely, a flat documentation directory might need all directory depth but only `--headings-depth 2` to show document shape without noise. A single `--depth` forces a choice that sacrifices one dimension unnecessarily.

### Alternative 4: Nested heading tree (H2 as child of H1, etc.)

**Description:** Model the JSON output so that `##` heading nodes are nested under the preceding `#` heading node, mirroring a rendered table of contents.

**Strengths:** JSON structure directly models document semantic hierarchy; useful for consumers that want to navigate TOC-style.

**Why not selected:** Produces an "orphan" handling problem when heading levels skip (H3 directly after H1 is valid Markdown but has no H2 parent). Requiring implementers to define orphan policy adds complexity without a clear correct answer. A flat list with `level` field is sufficient for FR-4 and simpler for downstream tooling to consume without recursion. See ADR-0011 for the output contract decision.

---

## Trade-Offs

| Gain | Cost |
|------|------|
| Zero new NuGet dependencies — ATX regex is self-contained | Setext headings silently omitted; repos using them show affected files as heading-free |
| Flat heading list is simple and deterministic, even for malformed sequences | JSON consumers cannot query heading parent-child relationships without re-processing by `level` |
| Single traversal produces both text and JSON output | Text renderer must carry heading-level state (`#` × level) |
| Dual depth controls give independent scoping of filesystem and heading dimensions | Two flags to document per RFC-0003 help-text conventions |
| Separate `HeadingExtractor` component is unit-testable without filesystem setup | Adds a component boundary that must be maintained |
| Directory-first alphabetical sort is deterministic across platforms | Sort order is not natural (OS-default) order; users expecting mtime or size sort will not get it |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Heading regex false-positives inside indented code blocks | Low | Low | Code-fence guard covers fenced blocks; indented code blocks (4-space) are uncommon and the risk is cosmetic only |
| Large repositories produce unmanageable output | Med | Med | `--max-depth` and `--headings-depth` are the intended mitigation; document recommended values in help text |
| Platform path separator differences cause non-deterministic `path` values in JSON | Low | High | Normalize all paths to forward slashes before emitting; apply the same normalization used elsewhere in ARS |
| ARS ignore behavior API does not exist or differs across commands | Med | Low | FR-12 is "should"; if the ignore API is absent or incompatible, the first release may skip this and track it as a follow-up |

---

## Testing / Validation Strategy

### Unit Tests — `HeadingExtractor`

| Test | What is verified |
|------|-----------------|
| ATX headings H1–H6 extracted in document order | Level and text correct for each |
| Headings inside fenced code blocks skipped | Fence guard works for ` ``` ` and `~~~` |
| `--headings-depth 2` filters H3+ | Depth filter applied correctly |
| Empty `.md` file returns empty list | No crash, no headings |
| File with only non-heading content returns empty list | No crash, no headings |
| File with trailing `#` closing syntax | Title correctly stripped |
| File with only fenced code blocks containing `#` lines | No headings extracted |
| Unreadable file returns empty list with no exception | `error: true` on file node |

### Unit Tests — `OutlineScanner`

| Test | What is verified |
|------|-----------------|
| Root with no `.md` files | Directory tree with file nodes, no heading children |
| Root with `.md` files | Heading children present for each `.md` file |
| `--max-depth 0` | Root node only, no children |
| `--max-depth 1` | Root and direct children only |
| Alphabetical + directory-first sort | Consistent across runs |
| `--path` pointing to a single `.md` file | File node as root with heading children |

### Snapshot / Golden Tests

| Fixture | Covers |
|---------|--------|
| `fixture-root-traversal/` — mixed files and dirs with `.md` files | Default traversal, text output |
| `fixture-subtree/` — scoped to `docs/` subdirectory | `--path` scoping |
| `fixture-max-depth/` — deep directory tree | `--max-depth` cutoff |
| `fixture-headings-depth/` — `.md` files with H1–H6 | `--headings-depth` filter |
| `fixture-headingless/` — `.md` files with no headings | Graceful file node output |
| `fixture-malformed/` — `.md` with broken heading syntax | Degraded output, no crash |
| `fixture-json/` — same as root traversal | `--format json` byte-stable output |

### Acceptance Tests (mapped to PRD-0002 acceptance criteria)

| PRD Criterion | Test |
|---------------|------|
| Default run outputs combined tree with `.md` headings inline | `fixture-root-traversal` snapshot |
| `--path <subtree>` limits traversal | `fixture-subtree` snapshot |
| `--max-depth <n>` omits deeper nodes | `fixture-max-depth` snapshot |
| `--headings-depth <n>` filters deep headings | `fixture-headings-depth` snapshot |
| Headingless file appears without children | `fixture-headingless` snapshot |
| Malformed Markdown does not crash | `fixture-malformed` snapshot |
| `--format json` produces valid typed node tree | `fixture-json` snapshot + JSON schema validation |
| Invalid path exits non-zero with clear error | Error-path test with non-existent path |
| Repeated runs produce identical output | Two runs against same fixture, byte comparison |

---

## Open Questions

- [ ] **Sort order within a directory level:** Should non-`.md` files and `.md` files be interleaved alphabetically (e.g., `CHANGELOG.md`, `README.md`, `guides/` alphabetically merged), or should directories always precede files? The proposed design uses directory-first, then files alphabetically. Reviewers should confirm this matches user expectations and aligns with any existing ARS traversal conventions.
- [ ] **`--path` pointing to a `.md` file directly:** The PRD edge-case table includes this scenario. The proposed design emits the file as the root node with heading children. This requires `OutlineScanner` to detect file vs. directory input. Confirm this is the correct behavior before implementation.
- [ ] **ARS/model-native ignore behavior scope:** FR-12 says the command "should reuse ARS/model-native ignore handling." Reviewers should confirm whether an ignore-filter API exists in the current codebase, what files it excludes, and whether it applies uniformly to the `outline` traversal or only to `compare`. If the API does not exist, FR-12 can be deferred to a follow-up.

---

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0002 — Repository Documentation Outline View](../prd/PRD-0002-repository-documentation-outline-view.md) | Product requirements this RFC implements |
| [ADR-0009 — Add `ars outline` to the ARS Command Surface](../adr/ADR-0009-add-ars-outline-command.md) | Records the decision to add the command |
| [ADR-0010 — ATX Headings Only in v1](../adr/ADR-0010-atx-headings-only-extraction.md) | Records the heading extraction scope decision |
| [ADR-0011 — Flat Heading List Under File Nodes](../adr/ADR-0011-flat-heading-list-under-file-nodes.md) | Records the output contract structure decision |
| [RFC-0001 — CLI Architecture](RFC-0001-cli-architecture.md) | Existing command registration and CLI structure this RFC extends |
| [RFC-0003 — CLI Ergonomics and Discoverability Baseline](RFC-0003-cli-ergonomics.md) | Help-output conventions that `ars outline` options must satisfy |
| [ADR-0004 — Keep v1 Read-Only Except for `init`](../adr/ADR-0004-keep-v1-readonly.md) | Confirms `ars outline` may not mutate repository files |
| [ADR-0006 — Define Comparison Semantics](../adr/ADR-0006-comparison-semantics.md) | Defines compare/report semantics that `ars outline` must not redefine |
