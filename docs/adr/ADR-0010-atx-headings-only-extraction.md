# ADR-0010: Scope Heading Extraction to ATX Headings Only in v1

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-29 |
| **Owner(s) / Deciders** | — |
| **Related RFC** | [RFC-0007 — `ars outline` Command Design](../rfc/RFC-0007-ars-outline-command-design.md) |
| **Related Links** | [PRD-0002 — Repository Documentation Outline View](../prd/PRD-0002-repository-documentation-outline-view.md) |

---

## Decision

Extract only ATX-style Markdown headings (`# H1`, `## H2`, through `###### H6`) in the `ars outline` v1 implementation. Setext-style headings (underline syntax) are not extracted.

## Context

Markdown defines two heading syntaxes:

- **ATX:** `#`-prefixed lines — `# Title`, `## Subtitle`, up to `######` for H6. Universal in modern Markdown tooling and GitHub-Flavored Markdown.
- **Setext:** underline-delimited lines — a text line followed by `===` (H1) or `---` (H2). Present in the original Markdown spec; limited to H1 and H2; rarely used in contemporary documentation.

`ars outline` must extract headings from `.md` files as part of the combined discovery view. Choosing the extraction strategy involves a trade-off between completeness and implementation simplicity. This decision also affects whether a Markdown parsing dependency is required.

## Options Considered

### Option 1: ATX headings only via line-by-line regex (Chosen)

**Description:** Scan each file line-by-line. Match lines that start with one to six `#` characters followed by a space and a non-empty title. Track code-fence state (` ``` ` and `~~~` blocks) to avoid false positives inside fenced code.

```
Regex: ^(#{1,6})\s+(.+?)(\s+#+\s*)?$
```

**Pros:**
- Zero new NuGet dependencies
- Simple, fast, independently testable
- Unambiguous — ATX headings cannot be confused with thematic breaks, list items, or any other Markdown element
- Covers all heading levels H1–H6
- Aligns with GitHub-Flavored Markdown, which is the dominant standard for `.md` files in open-source and enterprise repositories
- Code-fence guard is the only non-trivial state to track; implementation is a simple boolean

**Cons:**
- Setext headings are silently omitted; a file using setext syntax will appear heading-free in `ars outline` output

### Option 2: Full CommonMark parser (e.g., Markdig)

**Description:** Add a Markdown parsing library as a NuGet dependency. Parse each `.md` file into a CommonMark AST and extract heading nodes. Markdig is the leading .NET Markdown library and supports CommonMark + GFM extensions.

**Pros:**
- Handles all heading syntax including setext, ATX with leading spaces (up to 3), and inline HTML headings
- Future-proof: the same AST could later support link extraction, front matter parsing, or other analysis
- No custom parsing logic to maintain

**Cons:**
- Adds a runtime dependency requiring version pinning and license review
- Parses the entire document to a full AST when only heading nodes are needed — disproportionate to the extraction goal
- Increases binary size and startup time
- Adds a transitive dependency management burden for a capability that a simple regex handles adequately in the common case

### Option 3: Regex for both ATX and setext headings

**Description:** Use two regex patterns — one for ATX, one for setext (detecting a text line followed by `===` or `---`) — to cover both heading styles without a parser dependency.

**Pros:**
- No new dependency
- Covers both heading styles

**Cons:**
- Setext detection is inherently ambiguous: `---` is also a thematic break, and distinguishing it from a setext H2 underline requires tracking the previous non-blank line — which replicates a material portion of a real parser's state machine
- Handling edge cases correctly (blank lines between title and underline, mixed indentation) approaches the complexity of a real parser without its reliability
- Setext headings are limited to H1 and H2; the gain is minimal against the implementation risk

## Rationale

The extraction goal for v1 is practical coverage of heading structure in real-world repositories, not full CommonMark spec compliance. ARS's own documentation, GitHub repositories, and the overwhelming majority of modern `.md` files use ATX headings exclusively. Setext headings are a Markdown artifact from 2004; virtually no contemporary tooling generates them by default.

The code-fence guard is the one meaningful edge case for ATX heading extraction, and it is a straightforward boolean state tracked per file. This is not a custom parser — it is a targeted scan that is correct for the common case.

Adding Markdig (or equivalent) for this purpose would be disproportionate: the library would be used only to walk heading nodes while ignoring the rest of the AST. The dependency cost — versioning, license, binary size — is not justified for v1.

This decision can be revisited in a later release if setext support proves necessary for user-reported repositories. The `HeadingExtractor` component is isolated and independently replaceable.

## Consequences

### Positive

- Zero new NuGet dependencies in the ARS project
- `HeadingExtractor` is independently unit-testable with simple string fixtures
- Extraction behavior is fully predictable and documentable in help text ("ATX headings only")
- No binary size or startup time impact

### Negative

- Repositories that use setext headings will show affected `.md` files as heading-free in `ars outline` output
- No warning or indication is emitted when setext headings are encountered (consistent with PRD-0002's graceful-degradation policy, which accepts that unrecognized content is silently skipped)

## Risks / Follow-Ups

| Item | Type | Owner | Status |
|------|------|-------|--------|
| Monitor user-reported cases of missing headings due to setext syntax | Risk | ARS maintainers | Open |
| If setext support proves necessary, evaluate adding Markdig as a dependency in v1.x | Follow-Up | ARS maintainers | Pending |

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0002 — Repository Documentation Outline View](../prd/PRD-0002-repository-documentation-outline-view.md) | Product requirements including graceful-degradation policy |
| [RFC-0007 — `ars outline` Command Design](../rfc/RFC-0007-ars-outline-command-design.md) | Design RFC that specifies the `HeadingExtractor` algorithm |
