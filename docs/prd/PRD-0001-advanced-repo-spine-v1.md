# PRD-0001 — Advanced Repo Spine v1

## Document Metadata

- **Product name:** Advanced Repo Spine
- **Slug:** `advanced-ars`
- **Working short name:** `ARS`
- **Version:** `v1.0`
- **Status:** Approved
- **Type:** Product Requirements Document
- **Primary form factor:** Cross-platform CLI
- **Target runtime:** .NET
- **CLI framework:** Spectre.Console + Spectre.Console.Cli

---

## 1. Executive Summary

Advanced Repo Spine is a cross-platform CLI that makes repository structure explicit, inspectable, and machine-usable.

Its core purpose is to let contributors and AI coding agents understand the intended structure of a repository through a JSON model, compare that model with the actual working directory, and receive actionable guidance about missing, misplaced, extra, or undocumented structure elements.

Version 1 focuses on the structural backbone only:

- define a repository structure model
- initialize a starter model
- load and validate the model
- compare the model against a real repository
- generate human-readable and machine-readable reports
- suggest where a new path should live according to the model

Version 1 does **not** attempt to edit repository files, auto-generate markdown content, create TOCs, rewrite documents, or perform auto-fix operations.

---

## 2. Problem Statement

Repositories often evolve without a single explicit, machine-readable representation of their intended structure. Over time this creates several problems:

- contributors do not know where new files should go
- AI coding agents must infer structure from scattered conventions
- documentation, specs, ADRs, and operational artifacts drift into inconsistent locations
- repository hygiene becomes manual and subjective
- structure reviews are slow and error-prone
- onboarding becomes harder as projects grow

Most repositories contain more than source code. They also contain architecture notes, decisions, proposals, requirements, runbooks, reference material, and contributor guidance. These artifacts are important, but they are often not modeled or validated as first-class structure.

Advanced Repo Spine addresses this by treating repository layout as intentional design rather than incidental file placement.

---

## 3. Product Vision

Advanced Repo Spine provides a structured, machine-readable backbone for understanding, validating, and guiding the shape of a repository.

It should help both humans and AI answer questions such as:

- What is the intended structure of this repo?
- What is missing?
- What is misplaced?
- What does this folder represent?
- Where should I put a new file of a given kind?
- Does the current repo still match its intended design?

The long-term vision is broader, but v1 is deliberately narrow: make repository structure explicit, inspectable, and reportable.

---

## 4. Product Pitch

**The structural backbone for repositories built by humans and AI.**

Alternative pitch:
**A CLI that explains, validates, and guides repository structure.**

---

## 5. Goals for v1

### 5.1 Primary Goals

1. Allow users to create a starter repository structure model.
2. Allow users to define an intended repo structure in JSON.
3. Allow users to compare an actual working directory against that model.
4. Produce clear reports for missing, extra, misplaced, and unmatched items.
5. Provide path guidance for contributors and AI agents.
6. Produce machine-readable output suitable for downstream automation and agent use.
7. Stay deterministic, cross-platform, and CLI-first.

### 5.2 Success Criteria

v1 is successful if a contributor or agent can:

- initialize a usable model quickly
- validate that a model is syntactically correct
- run a comparison against a repo
- understand what is wrong or incomplete
- receive a clear recommendation about where something should go
- consume the results in JSON without guessing or screen scraping

---

## 6. Non-Goals for v1

The following are explicitly out of scope for v1:

- markdown editing
- automatic TOC generation
- automatic index file generation
- automatic repo fixes or file moves
- automatic folder creation from comparison results
- direct mutation of existing repository content
- support for YAML as the structure model format
- plugin architecture
- front matter parsing
- repository semantic analysis based on code content
- git history analysis
- IDE extension support
- live watch mode
- rich TUI workflows beyond standard CLI reporting
- multi-repo orchestration
- remote repository analysis without a local working directory

These features may become relevant in v2+.

---

## 7. Target Users

### 7.1 Primary Users

- software contributors working in structured repositories
- senior engineers defining repository conventions
- AI coding agents that need deterministic repository guidance
- maintainers reviewing repository hygiene and structure drift

### 7.2 Secondary Users

- tech leads onboarding new contributors
- platform or architecture owners standardizing internal repos
- tooling authors integrating structure validation into CI

---

## 8. Primary Use Cases

### 8.1 Initialize a New Model

A user wants to start modeling repository structure from scratch.

### 8.2 Validate Structure Intent

A maintainer wants to ensure the model is syntactically and semantically valid before using it.

### 8.3 Compare Model vs Actual Repo

A contributor wants to know whether the current repo matches its intended structure.

### 8.4 Generate a Report

A user wants a human-readable or JSON report describing structural issues.

### 8.5 Ask for Placement Guidance

A contributor or agent wants to know where a new file or folder should live.

### 8.6 Automate Checks

A CI workflow or coding agent wants to consume results programmatically.

---

## 9. Product Scope for v1

v1 includes four major capabilities:

### 9.1 Structure Model Lifecycle

- initialize a starter JSON model
- load a JSON model from disk
- validate the model structure and semantics

### 9.2 Repository Comparison

- scan a working directory
- compare scanned structure to the intended model
- identify missing, extra, misplaced, unknown, or unmatched items

### 9.3 Guidance

- provide a best-fit structural suggestion for a given path or intended artifact
- explain expected locations and matching rules

### 9.4 Reporting

- console-friendly report output
- machine-readable JSON output
- deterministic exit behavior suitable for automation

---

## 10. Functional Requirements

## 10.1 Command: `init`

Creates a starter JSON model file.

### 10.1.1 Requirements

- create a minimal valid JSON structure model
- support writing to a specified file path
- avoid overwriting an existing file unless explicitly forced
- optionally include commented guidance via adjacent markdown or example fields if comments inside JSON are not possible
- produce a simple starter structure, not a large opinionated template

### 10.1.2 Expected Outcome

A user can bootstrap the model without manually inventing the JSON format.

---

## 10.2 Command: `validate`

Validates a JSON model file.

### 10.2.1 Requirements

- verify JSON syntax
- verify required fields
- verify field types
- verify semantic constraints such as duplicate identifiers, invalid path patterns, or inconsistent nesting
- return non-zero exit code on validation failure
- show clear error locations/messages where possible

### 10.2.2 Expected Outcome

A user can trust that the model is usable before running comparisons.

---

## 10.3 Command: `compare`

Compares an actual repository structure against the model.

### 10.3.1 Requirements

- load a specified or default model file
- scan a target directory recursively
- compare actual paths against modeled expectations
- identify:
  - missing expected items
  - extra unmatched items
  - misplaced items when they appear to belong elsewhere
  - optional items that are absent but not errors
- support excluding ignored paths if configured in the model
- produce deterministic ordering in output

### 10.3.2 Expected Outcome

A user gets a stable structural diff between intent and reality.

---

## 10.4 Command: `report`

Displays or exports the results of a comparison in a focused report format.

### 10.4.1 Requirements

- consume either a fresh comparison or persisted comparison result if supported later
- present concise human-readable summaries
- optionally group results by severity or category
- support output format selection
- be easy to read in CI logs as well as local terminal use

### 10.4.2 Expected Outcome

A user can understand repo health without reading raw JSON.

---

## 10.5 Command: `suggest`

Suggests the expected location for a path, artifact, or structural intent.

### 10.5.1 Requirements

- accept a path or path-like hint
- optionally accept a name or category hint
- match against model rules and return the best candidate locations
- explain why the suggestion was made
- return no match clearly when the model cannot determine placement

### 10.5.2 Expected Outcome

A contributor or AI agent can ask where something belongs.

---

## 10.6 Command: `export`

Exports structured comparison or model-derived results in machine-readable format.

### 10.6.1 Requirements

- support JSON output in v1
- expose key comparison details in a stable shape
- be deterministic and script-friendly
- avoid embedding terminal formatting or presentation concerns

### 10.6.2 Expected Outcome

Other tools and AI agents can consume outputs reliably.

---

## 10.7 Command: `config` (Optional for v1)

This command is only in scope if configuration cannot be handled cleanly through model file options alone.

### 10.7.1 Recommendation

For v1, prefer keeping configuration inside the model file and avoid a separate mutable config system unless clearly necessary.

---

## 11. Recommended v1 CLI Surface

A practical v1 command set is:

```text
ars init
ars validate
ars compare
ars report
ars suggest
ars export
````

### Suggested high-level argument examples

```text
ars init --path ars.json
ars validate --model ars.json
ars compare --model ars.json --root .
ars report --model ars.json --root . --format text
ars suggest docs/adr/0001-use-json.md --model ars.json
ars export --model ars.json --root . --format json
```

### Notes

- `compare` and `export` may overlap; the exact split should be finalized in implementation design.
- A simpler design is also acceptable:
  - `compare` does the analysis
  - `--format text|json` controls output
  - `report` becomes optional or a presentation alias

This should be resolved through an ADR before implementation starts.

---

## 12. User Experience Requirements

### 12.1 CLI Principles

- fast startup
- clear error messages
- deterministic output ordering
- clean cross-platform path handling
- suitable for humans and automation
- no hidden state by default

### 12.2 Human Output

Human-readable output should:

- clearly separate summary from details
- distinguish errors, warnings, and informational notes
- explain why something failed to match
- avoid overly verbose raw dumps unless requested

### 12.3 Machine Output

Machine-readable output should:

- use stable field names
- avoid presentation-only fields
- include enough structure for downstream automation
- be predictable across platforms

---

## 13. Technical Constraints

### 13.1 Runtime and Language

- .NET
- C#
- cross-platform execution on Windows, Linux, and macOS

### 13.2 CLI Stack

- Spectre.Console
- Spectre.Console.Cli

### 13.3 Serialization

- JSON is the only supported model format in v1
- `System.Text.Json` is preferred unless a stronger implementation-specific reason emerges

### 13.4 File System Behavior

- support platform differences in path separators
- define case-sensitivity behavior explicitly
- normalize paths consistently before comparison

---

## 14. Repository Structure Model

## 14.1 Model Design Principles

The structure model should be:

- hierarchical
- explicit
- deterministic
- human-authorable
- machine-readable
- minimal for v1
- expressive enough to model folders, files, optionality, and nesting

## 14.2 Required Capabilities in the Model

The model should support:

- repository root definition
- expected directories
- expected files
- nested child items
- required vs optional items
- simple descriptive metadata
- rules for ignoring certain paths
- stable identifiers or names for nodes where useful

## 14.3 Proposed Top-Level Shape

```json
{
  "version": "1.0",
  "name": "Example Repository Model",
  "description": "Structure model for the repository.",
  "rules": {
    "caseSensitive": false,
    "ignore": [
      ".git/",
      "bin/",
      "obj/"
    ]
  },
  "items": [
    {
      "name": "docs",
      "type": "directory",
      "path": "docs",
      "required": true,
      "description": "Repository documentation root.",
      "children": [
        {
          "name": "architecture",
          "type": "directory",
          "path": "docs/architecture",
          "required": true
        },
        {
          "name": "readme",
          "type": "file",
          "path": "README.md",
          "required": true
        }
      ]
    }
  ]
}
```

---

## 15. Proposed Model Schema Semantics

## 15.1 Root Object

### Fields

- `version` — model schema version
- `name` — model display name
- `description` — optional explanatory text
- `rules` — global comparison rules
- `items` — top-level expected structure entries

## 15.2 Rules Object

### 15.2.1 Candidate fields

- `caseSensitive` — whether path matching is case-sensitive
- `ignore` — list of ignored path patterns or explicit paths

v1 should keep this intentionally small.

## 15.3 Item Object

### 15.3.1 Candidate fields

- `name` — human-friendly node name
- `type` — `directory` or `file`
- `path` — expected repository-relative path
- `required` — whether absence is an error
- `description` — optional guidance text
- `children` — nested items for directories

### Optional future fields, not required for v1

- `tags`
- `aliases`
- `allowedPatterns`
- `placementHints`
- `ownership`
- `examples`
- `extensions`
- `constraints`

These should remain out of scope unless proven necessary for v1.

---

## 16. Comparison Semantics

v1 comparison should distinguish at least the following result types:

### 16.1 Missing

Expected by model but not found in actual repo.

### 16.2 Present

Expected and found.

### 16.3 Optional Missing

Defined as optional and absent.

### 16.4 Extra / Unmatched

Found in repo but not matched to any model entry.

### 16.5 Misplaced

Found in repo, but appears to represent a modeled item that is expected elsewhere.

Misplacement logic should be conservative in v1. It is better to classify as unmatched than to make overconfident relocation claims.

---

## 17. Reporting Requirements

## 17.1 Human Report Must Include

- model name
- target root path
- summary counts
- missing items
- unmatched items
- misplaced items if detected
- optional absent items separately from hard failures

## 17.2 JSON Export Must Include

- model metadata
- target root metadata
- timestamp
- normalized comparison settings
- summary counts
- detailed findings array
- per-finding type, severity, expected path, actual path where applicable, and explanation

### Example JSON result shape

```json
{
  "modelVersion": "1.0",
  "modelName": "Example Repository Model",
  "root": ".",
  "summary": {
    "missing": 2,
    "unmatched": 5,
    "misplaced": 1,
    "optionalMissing": 3
  },
  "findings": [
    {
      "type": "missing",
      "severity": "error",
      "expectedPath": "docs/architecture",
      "actualPath": null,
      "message": "Required directory is missing."
    }
  ]
}
```

---

## 18. Exit Code Requirements

CLI commands should use predictable exit codes.

### Suggested behavior

- `0` — success, no blocking issues
- non-zero — validation errors, comparison failures, invalid usage, or runtime failures

A more detailed exit code strategy may be defined during implementation, but v1 must at least distinguish success from failure.

---

## 19. Acceptance Criteria

## 19.1 `init`

- user can generate a valid starter JSON model
- generated file passes `validate`
- existing files are not overwritten accidentally

## 19.2 `validate`

- invalid JSON is rejected
- semantically invalid models are rejected
- valid models pass cleanly

## 19.3 `compare`

- command works on a local repository root
- detects missing required items
- detects unmatched extra items
- respects ignore rules
- produces deterministic output

## 19.4 `report`

- produces readable summary and details
- clearly distinguishes blocking vs informational findings

## 19.5 `suggest`

- returns plausible destination guidance for known modeled areas
- explains when no confident suggestion is available

## 19.6 `export`

- produces stable JSON output
- includes enough detail for downstream tools or agents

## 19.7 Cross-platform

- tool runs on Windows, Linux, and macOS
- path normalization behaves consistently
- no platform-specific assumptions break core functionality

---

## 20. Quality Requirements

### 20.1 Determinism

The same input model and same repo state should produce the same ordered output.

### 20.2 Safety

v1 should be read-only with respect to repository content, except for explicitly writing the initialized model file.

### 20.3 Clarity

All error and guidance output should be understandable without reading source code.

### 20.4 Testability

The core comparison engine should be independently testable from the CLI shell.

### 20.5 Separation of Concerns

Implementation should separate:

- CLI layer
- model parsing/validation
- filesystem scanning
- comparison engine
- reporting/export

---

## 21. Recommended Internal Architecture

A clean v1 implementation should have distinct layers:

### 21.1 Domain Layer

Contains model objects and comparison result types.

### 21.2 Parsing / Validation Layer

Loads JSON and validates schema semantics.

### 21.3 Filesystem Scanning Layer

Builds normalized actual-repo structure representations.

### 21.4 Comparison Engine

Matches actual structure against intended structure and produces findings.

### 21.5 Reporting Layer

Formats results for console and JSON output.

### 21.6 CLI Layer

Maps user input to domain operations using Spectre.Console.Cli.

This separation will make future expansion easier without overengineering v1.

---

## 22. Risks and Tradeoffs

## 22.1 JSON vs YAML

JSON is less pleasant for some humans to author, but simpler, stricter, and more consistent for tooling and AI parsing. This tradeoff is acceptable for v1.

## 22.2 Misplacement Detection Complexity

Automatically deciding that something is misplaced can be error-prone. v1 should use conservative logic and prioritize correctness over aggressive inference.

## 22.3 Over-Scoping

There is strong temptation to add markdown editing and automation features. This would dilute the core product and should be resisted in v1.

## 22.4 Model Expressiveness

If the model is too simple, it may not represent real repos well. If too complex, it becomes hard to author and reason about. v1 should optimize for a small, usable middle ground.

---

## 23. Out-of-Scope Features for v2+ Backlog

These are strong candidates for future versions, but not v1:

- markdown TOC generation
- markdown index generation
- deterministic markdown editing actions
- front matter parsing
- YAML model support
- auto-fix mode
- file move recommendations with confirmation flow
- plugin architecture
- richer pattern rules
- model merging from multiple files
- repo templates and model presets
- CI annotations
- richer guidance text and examples
- interactive TUI workflows
- documentation artifact generation
- import from existing repo structure
- path ownership and responsibility metadata
- support for detecting documentation taxonomy gaps

---

## 24. ADRs Recommended Alongside v1

The following ADRs should be created during or before implementation:

### ADR 0001 — Use .NET for a Cross-Platform CLI

Records why .NET is the runtime and language platform.

### ADR 0002 — Use Spectre.Console and Spectre.Console.Cli

Records the CLI framework choice and rationale.

### ADR 0003 — Use JSON as the v1 Model Format

Records why JSON was chosen over YAML or other formats.

### ADR 0004 — Keep v1 Read-Only Except for `init`

Records the decision to avoid mutation features in v1.

### ADR 0005 — Define the v1 Repository Structure Model

Records the model shape, semantics, and constraints.

### ADR 0006 — Define Comparison Semantics

Records how missing, unmatched, optional, and misplaced are interpreted.

### ADR 0007 — Documentation Conventions for Project Artifacts

Records use of Markdown and any metadata conventions for project docs.

---

## 25. RFCs vs ADRs Guidance for This Project

For this project:

- write an **RFC** when the team is still exploring alternatives or requesting feedback
- write an **ADR** when a technical decision is made and should become part of the project record

### Examples

- whether to support YAML in addition to JSON -> RFC first
- final decision to use JSON only in v1 -> ADR
- whether `report` should exist as a separate command -> RFC first if undecided
- final CLI command surface for v1 -> ADR

---

## 26. Open Questions

The following should be resolved before implementation is fully locked:

1. Should `report` be a separate command or just `compare --format text`?
2. Should `export` be separate or just `compare --format json`?
3. How much path pattern flexibility is needed in v1?
4. Should file matching be exact-path only in v1, or allow simple wildcards?
5. How should case sensitivity behave across platforms by default?
6. Should the model support node IDs in v1 or rely only on paths and names?
7. Should there be a default conventional model template in `init`, or only an empty starter?

---

## 27. Final v1 Recommendation

Keep v1 narrowly focused and finishable.

The best v1 is:

- a solid JSON model
- a dependable validator
- a deterministic comparison engine
- clear CLI reporting
- basic placement guidance
- clean machine-readable export

Anything that edits markdown, generates docs, or mutates the repo should wait until the structure engine is proven.

---

## 28. One-Sentence Definition

Advanced Repo Spine v1 is a cross-platform .NET CLI that uses a JSON model to explain, validate, and guide repository structure for humans and AI agents.
