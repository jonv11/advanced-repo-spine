# Release Process

This document describes how releases are prepared and published for Advanced Repo Spine.

## Pre-release checklist

Before preparing a release, verify the following:

- [ ] All tests pass: `dotnet test Ars.sln -c Release`
- [ ] The build succeeds on all target platforms (CI green)
- [ ] No unresolved bugs tagged for this release
- [ ] Documentation is up to date for any changed behavior
- [ ] Breaking changes (if any) are documented and justified
- [ ] Version in `src/Ars.Cli/Ars.Cli.csproj` has been updated
- [ ] CHANGELOG.md has been updated with the new version entry

## Release steps

### 1. Prepare the release

```bash
# Create a release branch
git checkout main
git pull
git checkout -b release/vX.Y.Z

# Update the version in the project file
# Edit src/Ars.Cli/Ars.Cli.csproj: <Version>X.Y.Z</Version>

# Move CHANGELOG entries from "Unreleased" to the new version section
# Add the release date

# Commit the version bump and changelog
git add -A
git commit -m "chore: prepare release vX.Y.Z"
```

### 2. Validate

```bash
# Full build
dotnet build Ars.sln -c Release

# Full test suite
dotnet test Ars.sln -c Release --verbosity normal

# Smoke test the CLI
dotnet run --project src/Ars.Cli -c Release -- --help
dotnet run --project src/Ars.Cli -c Release -- --version
dotnet run --project src/Ars.Cli -c Release -- init --path ars.release-smoke.json --force
dotnet run --project src/Ars.Cli -c Release -- validate --model ars.release-smoke.json
```

`ars --version` may display SemVer build metadata such as `1.0.0+sha.1851306` when Git metadata is available. The release tag remains `vX.Y.Z`.

### 3. Merge and tag

```bash
# Open a PR from release/vX.Y.Z to main
# Ensure CI passes
# Merge the PR

# Tag the release on main
git checkout main
git pull
git tag -a vX.Y.Z -m "Release vX.Y.Z"
git push origin vX.Y.Z
```

### 4. Create GitHub Release (after GitHub repo exists)

- Go to the repository's Releases page
- Create a new release from the `vX.Y.Z` tag
- Use the CHANGELOG entry as the release description
- Mark as pre-release if using `-alpha`, `-beta`, or `-rc` suffix

## Version selection guidance

Refer to the [versioning policy](versioning-policy.md) for detailed rules. In summary:

| Change type | Version bump |
|-------------|-------------|
| Bug fixes, internal improvements | PATCH |
| New commands, options, model fields | MINOR |
| Breaking CLI, model, or output changes | MAJOR |

## Post-release

- Verify the GitHub Release is visible and correctly tagged
- Check that the release notes are accurate
- If applicable, announce the release in relevant channels

---

## First public release checklist

The following items must be resolved before the first public release:

- [x] **License selected and LICENSE file added** — Apache License 2.0; `LICENSE` added
- [x] **CODEOWNERS configured** — `.github/CODEOWNERS` with `@jonv11`
- [x] **README badges added** — CI status, license, latest release
- [x] **Branch protection rules configured** — PRs required, 1 approval, CI status checks, no force-push, no delete
- [x] **Issue labels created** — `bug`, `feature`, `docs`, `chore`, `breaking`, `good first issue`, `wontfix`, `duplicate`
- [x] **Security contact email set** — GitHub private vulnerability reporting
- [x] **Repository description and topics set** — configured on `jonv11/advanced-repo-spine`

---

## Items deferred until after GitHub publication

These steps become relevant only after the GitHub repository exists:

| Item | When to implement |
|------|-------------------|
| GitHub Release creation from tags | After first push to GitHub |
| Release automation workflow (tag → build → publish) | After release process is proven manually |
| NuGet package publication | After packaging decisions are finalized |
| GitHub Pages documentation hosting | After documentation publishing strategy is decided |
| Automated changelog generation from commits | After Conventional Commits are established in practice |
