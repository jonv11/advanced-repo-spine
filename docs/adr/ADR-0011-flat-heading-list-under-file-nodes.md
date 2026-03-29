# ADR-0011: Use a Flat Heading List Under File Nodes for `ars outline` Output

| Field | Value |
|-------|-------|
| **Status** | Proposed |
| **Date** | 2026-03-29 |
| **Owner(s) / Deciders** | — |
| **Related RFC** | [RFC-0007 — `ars outline` Command Design](../rfc/RFC-0007-ars-outline-command-design.md) |
| **Related Links** | [PRD-0002 — Repository Documentation Outline View](../prd/PRD-0002-repository-documentation-outline-view.md) |

---

## Decision

Model heading nodes as a flat, ordered list of children directly under their parent file node — not as a nested tree reflecting heading hierarchy — with an explicit `level` field (1–6) on each heading node.

## Context

`ars outline` produces a unified node tree of directories, files, and Markdown headings. For the heading dimension, there are two plausible structural models:

- **Flat list:** all headings extracted from a file appear as direct children of the file node, ordered by document position, each carrying a `level` field (1–6)
- **Nested tree:** heading nodes are nested to reflect document hierarchy — an `##` heading node becomes a child of the preceding `#` heading node

The choice affects the JSON schema contract, the text renderer, and the complexity of both the producer (scanner) and consumer (downstream tooling).

PRD-0002 FR-8 requires a "unified tree with explicit node types" — both models satisfy this. FR-4 requires that heading levels be distinguishable — both models satisfy this. The question is which model is simpler, more deterministic, and more useful for the stated consumer types.

## Options Considered

### Option 1: Flat list with `level` field (Chosen)

**Description:** Heading nodes are direct children of their parent file node in document order. Each heading node carries `level: 1–6`. No structural nesting is applied.

Example JSON:
```json
{
  "type": "file",
  "name": "guide.md",
  "path": "docs/guide.md",
  "children": [
    { "type": "heading", "level": 1, "name": "Guide" },
    { "type": "heading", "level": 2, "name": "Installation" },
    { "type": "heading", "level": 3, "name": "Prerequisites" },
    { "type": "heading", "level": 2, "name": "Usage" }
  ]
}
```

**Pros:**
- Production is straightforward: read headings in document order and append to the file's children list; no outline-hierarchy state required
- Deterministic even for malformed heading sequences (H3 after H1 with no H2 is valid Markdown; a flat list handles it without an orphan policy)
- Simple to consume: iterate `children` where `type === "heading"` to get all headings in order
- Text rendering is a direct iteration over the flat list — no recursion through heading subtrees
- Matches the existing ARS principle of deterministic, presentation-independent output (NFR-1, NFR-3)

**Cons:**
- JSON consumers cannot directly navigate the parent-child heading relationship without re-processing the flat list by `level` in sequence

### Option 2: Nested heading tree (H2 as child of H1, H3 as child of H2, etc.)

**Description:** Heading nodes are nested to reflect document semantic hierarchy. An `##` heading is added as a child of the most recently seen `#` heading node; an `###` heading is a child of the most recently seen `##` heading.

Example JSON:
```json
{
  "type": "file",
  "name": "guide.md",
  "path": "docs/guide.md",
  "children": [
    {
      "type": "heading", "level": 1, "name": "Guide",
      "children": [
        {
          "type": "heading", "level": 2, "name": "Installation",
          "children": [
            { "type": "heading", "level": 3, "name": "Prerequisites", "children": [] }
          ]
        },
        { "type": "heading", "level": 2, "name": "Usage", "children": [] }
      ]
    }
  ]
}
```

**Pros:**
- JSON directly models the semantic document outline — navigating to "all H2 sections under the Introduction" is a single tree traversal
- Mirrors how a table of contents is typically rendered

**Cons:**
- Requires an "orphan" handling policy when heading levels skip (e.g., H3 directly after H1 with no intervening H2). CommonMark does not forbid this; real documents do it. The policy choices (attach as child of H1, create a phantom H2 parent, emit at root level) are all defensible but none is obviously correct — reviewers would need to agree on one
- More complex to produce: requires a mutable heading-stack during extraction
- More complex to consume: requires recursion to iterate all headings in order; downstream tooling cannot simply filter `children` by type
- Text rendering requires recursive traversal of heading subtrees rather than a simple flat loop
- Adds state and complexity to `HeadingExtractor` that is not justified by the v1 use cases (contributors, agents, and technical leads scanning document shape)

### Option 3: Separate `headings` array field on file nodes

**Description:** File nodes carry both `children` (for sub-entries in the tree) and a separate `headings` array (for heading nodes). Heading nodes are not part of the unified tree.

**Pros:**
- JSON consumers can trivially distinguish between sub-directory/file entries and heading entries without checking `type`

**Cons:**
- Breaks the unified tree contract required by PRD-0002 FR-8 ("unified tree with explicit node types that include at least `directory`, `file`, and `heading`")
- Inconsistent with how directory children are modeled — creates two different traversal patterns in one schema
- Makes the text renderer more complex (two node lists per file instead of one)

## Rationale

The flat list satisfies all stated PRD requirements:

- FR-4: heading levels are distinguishable via the `level` field
- FR-8: the output is a unified tree with explicit `type` values including `heading`
- NFR-1: output is deterministic regardless of heading sequence in the source document
- NFR-3: the JSON contract is stable and presentation-independent

The nested tree model would be useful only if consumers regularly need to navigate heading hierarchy programmatically. The stated consumer types — contributors scanning document shape, AI agents loading planning context, technical leads inspecting documentation richness — all benefit from a flat, ordered list of headings that can be displayed or iterated directly. None of them requires navigating H2-children-of-H1 in a tree walk.

The orphan-handling problem in the nested model is a real implementation risk. A document like `# Title` followed immediately by `### Deep Heading` (skipping H2) is syntactically valid Markdown and occurs in practice. Defining orphan policy requires a design choice that has no clear correct answer and would need its own ADR — adding complexity without a justified use case.

## Consequences

### Positive

- `HeadingExtractor` returns a simple ordered list; no heading-stack state required
- JSON schema is simpler and stable: no `children` on heading nodes means no recursive schema
- Text rendering iterates the flat list once; no recursive subtree walk
- Deterministic for all valid heading sequences, including those with non-sequential levels
- Downstream tooling can reconstruct heading hierarchy from the flat list if needed, by reading `level` in order

### Negative

- JSON consumers cannot query "all H2 headings under the Introduction section" with a simple tree traversal — they must process the flat list and infer hierarchy from `level` sequence

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0002 — Repository Documentation Outline View](../prd/PRD-0002-repository-documentation-outline-view.md) | FR-4 (heading levels), FR-8 (unified typed-node tree), NFR-1 (deterministic), NFR-3 (stable JSON) |
| [RFC-0007 — `ars outline` Command Design](../rfc/RFC-0007-ars-outline-command-design.md) | Design RFC that specifies `OutlineNode` data model and JSON schema |
