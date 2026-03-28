# ADR-0003: Use JSON as the v1 Model Format

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-28 |
| **Owner(s) / Deciders** | — |
| **Related RFC** | [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) |
| **Related Links** | [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-ars-v1.md) |

---

## Decision

Use JSON as the only supported model format in v1. No YAML, TOML, or other formats.

## Context

The repository structure model is the core input artifact for ARS. Users author it by hand to describe intended repository structure. The tool parses it, validates it, and compares it against reality. The format choice affects authoring ergonomics, parsing reliability, tooling support, and AI-agent compatibility.

PRD-0001 §6 explicitly lists YAML model support as a non-goal for v1. PRD §22.1 acknowledges the trade-off: JSON is less pleasant for some humans to author, but simpler, stricter, and more consistent for tooling and AI parsing.

## Options Considered

### Option 1: JSON only (Chosen)

**Description:** The model file is a `.json` file parsed with System.Text.Json. No alternative formats are supported.

**Pros:**
- System.Text.Json is built into .NET — zero additional dependencies
- Strict syntax with no ambiguity (no implicit typing, no anchor/alias complexity)
- Universal tooling support (editors, linters, schema validators, formatters)
- AI agents parse and generate JSON reliably — it is the most common structured format in LLM training data
- Deterministic parsing — same input always produces the same result
- JSON Schema can be used for validation in editors

**Cons:**
- More verbose than YAML (braces, quotes, commas)
- No comments in standard JSON (workaround: `description` fields)
- Deeply nested structures become harder to read

### Option 2: YAML only

**Description:** Use YAML as the model format, parsed with a YAML library like YamlDotNet.

**Pros:**
- More concise and human-friendly for hand-authored files
- Supports comments natively
- Familiar to users of Kubernetes, GitHub Actions, and other YAML-heavy ecosystems

**Cons:**
- Adds an external dependency (YamlDotNet or similar)
- YAML parsing has well-documented pitfalls: implicit type coercion (`yes` → `true`, `1.0` → float), Norway problem, anchor/alias complexity
- Multiple valid representations for the same data (flow vs. block style)
- AI agents handle YAML less reliably than JSON — indentation and implicit types cause generation errors
- Non-deterministic serialization without careful configuration

### Option 3: Support both JSON and YAML

**Description:** Accept either format, detected by file extension.

**Pros:**
- Maximum user flexibility
- Users can choose the format they prefer

**Cons:**
- Two parsing paths to maintain and test
- Edge cases where behavior differs between formats
- Additional external dependency for YAML
- Documentation must cover both formats
- Feature parity must be maintained across formats
- Doubles the surface area for bugs

## Rationale

JSON's strictness is an advantage for a structural model where precision matters. The model defines expected paths, types, and hierarchy — ambiguity in parsing would undermine the tool's purpose. System.Text.Json's zero-dependency availability in .NET eliminates an external package.

The lack of comments is mitigated by the `description` field on every model item, which serves the same purpose (explaining intent) while remaining machine-readable. AI agents reliably parse and generate JSON, which directly supports the product's goal of being agent-friendly.

YAML support remains a strong v2 candidate (PRD §23) once the model schema is proven and stable.

## Consequences

### Positive

- Zero external parsing dependencies
- Deterministic, unambiguous parsing
- AI agents can reliably read and write model files
- JSON Schema can provide editor-time validation
- One format to document, test, and maintain

### Negative

- Users accustomed to YAML may find JSON verbose
- No inline comments — guidance must use `description` fields
- Deeply nested models are harder to read (mitigated by keeping v1 models shallow)

## Related Documents

| Document | Relationship |
|----------|-------------|
| [PRD-0001 — Advanced Repo Spine v1](../prd/PRD-0001-ars-v1.md) | YAML excluded in §6; trade-off discussed in §22.1 |
| [RFC-0001 — CLI Architecture](../rfc/RFC-0001-cli-architecture.md) | Model layer design assumes JSON-only |
| [ADR-0005 — v1 Model Schema](ADR-0005-v1-model-schema.md) | Defines the JSON schema structure |
