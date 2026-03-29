# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- GitHub Actions release asset workflow that publishes packaged binaries for Windows, Linux, and macOS and uploads them to GitHub Releases with SHA-256 checksums

## [1.0.0] — 2026-03-29

### Added

- `ars init` — create a starter JSON model (`ars.json`)
- `ars validate` — validate JSON model syntax and semantics
- `ars compare` — compare repository against model
- `ars report` — display comparison results (human-readable text)
- `ars suggest` — suggest location for a path based on the model
- `ars export` — export comparison results as JSON
- `ars outline` — display repository structure with Markdown heading outlines
- JSON repository structure model format with hierarchical recursive schema
- Five finding types: Present, Missing, OptionalMissing, Unmatched, Misplaced
- Conservative misplacement detection (single candidate, exact name match)
- Cross-platform support (Windows, Linux, macOS)
- Documentation pipeline: PRD → RFC → ADR with templates and interactive prompts
- 11 Architecture Decision Records documenting key design choices
- LICENSE file (Apache License 2.0)
- CODEOWNERS (`.github/CODEOWNERS`) — `@jonv11` owns all files
- README badges — CI status, license, latest release
- Repository governance and contributor infrastructure
  - CONTRIBUTING.md with quick-start guide
  - Development guides: branching/commits, versioning, release process, GitHub workflow
  - PR template, issue templates (bug report, feature request)
  - CI workflow (build and test on ubuntu, windows, macos)
  - Dependabot configuration for NuGet and GitHub Actions
  - Security policy (GitHub private vulnerability reporting) and support guide
  - .editorconfig, .gitattributes, global.json

### Changed

- `ars --version` reports SemVer build metadata with the short Git commit hash when Git metadata is available, for example `1.0.0+sha.1851306`
