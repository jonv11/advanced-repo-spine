# ADR-0005: Define the v1 Repository Structure Model Schema

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-28 |
| **Owner(s) / Deciders** | — |
| **Related RFC** | [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) |

---

## Decision

Use the hierarchical JSON model schema defined in PRD-0001 §14–15 and refined in RFC-0001, with three top-level objects (`version`, `rules`, `items`) and a recursive `ModelItem` type supporting `name`, `type`, `path`, `required`, `description`, and `children`.

## Context

The repository structure model is the core input for ARS. It must be expressive enough to represent real-world repository structures (directories, files, nesting, optionality) while remaining simple enough for humans to author by hand and for AI agents to generate reliably. PRD-0001 §14 defines the design principles: hierarchical, explicit, deterministic, human-authorable, machine-readable, minimal for v1.

The schema shape determines what the comparison engine can detect and what the suggestion engine can recommend. Over-engineering the schema delays v1; under-engineering it makes the tool useless for real repos.

## Options Considered

### Option 1: Hierarchical with recursive items (Chosen)

**Description:** A root object with metadata (`version`, `name`, `description`), global rules (`caseSensitive`, `ignore`), and a recursive `items` array where each item has `name`, `type`, `path`, `required`, `description`, and optional `children`.

```json
{
  "version": "1.0",
  "name": "My Repo",
  "rules": { "caseSensitive": false, "ignore": [".git/"] },
  "items": [
    {
      "name": "docs",
      "type": "directory",
      "path": "docs",
      "required": true,
      "children": [
        { "name": "readme", "type": "file", "path": "docs/README.md", "required": true }
      ]
    }
  ]
}
```

**Pros:**
- Mirrors actual filesystem hierarchy — intuitive mental model
- Nesting makes parent-child relationships explicit
- `required` flag per item enables fine-grained optionality
- `description` provides inline documentation without comments
- Recursive structure keeps the schema small (one item type)

**Cons:**
- Deeply nested repos produce deeply nested JSON
- `path` is redundant with hierarchy (child paths could be inferred) — but explicit paths prevent ambiguity
- No support for patterns, wildcards, or grouping (limits expressiveness)

### Option 2: Flat list with path-based hierarchy

**Description:** A flat array of items, each with a full path. Hierarchy is inferred from path prefixes.

```json
{
  "version": "1.0",
  "items": [
    { "path": "docs", "type": "directory", "required": true },
    { "path": "docs/README.md", "type": "file", "required": true }
  ]
}
```

**Pros:**
- No nesting — every item is at the same level
- Easier to sort, filter, and merge
- Simpler deserialization (no recursion)

**Cons:**
- Parent-child relationships must be inferred from path prefixes — error-prone
- No natural grouping of related items
- Harder for humans to author — structure is not visually evident
- Validation is harder (must verify path consistency across flat list)

### Option 3: Pattern-based schema

**Description:** Items defined using glob patterns and regex rather than explicit paths.

```json
{
  "items": [
    { "pattern": "docs/**/*.md", "type": "file", "required": true },
    { "pattern": "src/*/", "type": "directory", "required": true }
  ]
}
```

**Pros:**
- Very concise for repos with repeating structures
- Handles dynamic content (multiple modules, generated files)

**Cons:**
- Pattern semantics are complex and hard to validate
- Comparison logic becomes significantly more complex (regex/glob matching vs. path equality)
- Hard for humans to predict what a pattern will match
- AI agents generate regex unreliably
- PRD §22.4 explicitly warns against model complexity in v1

## Rationale

The hierarchical recursive structure maps naturally to filesystems and is intuitive for humans authoring models. Explicit `path` fields (rather than inferring from hierarchy) eliminate ambiguity and make comparison straightforward: model paths are compared directly against scanned paths. The `required` flag per item enables mixed mandatory/optional structures, which reflects real repos where some files are critical and others are conventional.

The flat-list approach was rejected because it loses visual hierarchy, making models harder to author and review. The pattern-based approach was rejected because it introduces complexity disproportionate to v1's needs — explicit paths cover the common case, and patterns can be added in v2 as an optional field.

Fields listed in PRD §15.3.1 as "optional future fields" (`tags`, `aliases`, `allowedPatterns`, `placementHints`, `ownership`, `examples`, `extensions`, `constraints`) are explicitly excluded from v1 but the schema is forward-compatible: unknown fields are ignored during parsing.

## Consequences

### Positive

- Simple, predictable schema — one recursive type handles all nesting
- Direct path comparison — no pattern matching complexity
- Human-authorable — structure mirrors the repo it describes
- Forward-compatible — new fields can be added without breaking v1 models
- `description` fields serve as documentation, compensating for JSON's lack of comments

### Negative

- No pattern or wildcard support — repos with many similarly-structured modules need verbose models
- Deep nesting produces visually dense JSON
- Explicit `path` on every item is redundant with hierarchy — but this is intentional for clarity

## Amendment: Case-Sensitivity Scope (2026-03-29)

The `rules.caseSensitive` flag controls whether path matching treats uppercase and lowercase characters as equivalent. This amendment clarifies the exact scope of that flag across all operations that compare or inspect model paths.

### Comparison path matching

The comparison engine (ADR-0006) uses `caseSensitive` to select the `StringComparer` for its path-lookup dictionary. When `caseSensitive` is `false`, `README.md` and `readme.md` resolve to the same model item. This is the primary use of the flag and is implemented correctly.

### Validation duplicate-path detection

The model validator must detect duplicate paths using the same case-sensitivity semantics as the comparison engine. When `caseSensitive` is `false`, two items at paths `README.md` and `readme.md` are duplicates — only one would survive in the comparison dictionary, and the other would be silently dropped. The validator's `seenPaths` set must use a `StringComparer` consistent with the model's `caseSensitive` flag, not an unconditional `StringComparer.Ordinal`.

### Ignore pattern matching

Ignore patterns (`rules.ignore`) are always matched case-insensitively, regardless of the `caseSensitive` flag. This is a deliberate v1 simplification: ignore lists typically contain platform artifacts (`.git/`, `bin/`, `obj/`) where case variation is accidental, and making ignore matching case-sensitive would create cross-platform ergonomics issues (e.g., `.Git/` on macOS vs. `.git/` on Linux). Future versions may revisit this if use cases require case-sensitive ignore patterns.

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-advanced-repo-spine-v1.md) | Model design in §14–15; expressiveness trade-off in §22.4 |
| [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) | Type definitions for `RepoModel`, `ModelItem`, `ModelRules`; validation rules |
| [ADR-0003 — Use JSON](ADR-0003-use-json-model-format.md) | Format choice this schema is expressed in |
| [ADR-0006 — Comparison Semantics](ADR-0006-comparison-semantics.md) | How this schema is used during comparison |
