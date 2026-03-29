# Security Policy

## Supported versions

| Version | Supported |
|---------|-----------|
| 1.0.x   | Yes       |

This table will be updated as new versions are released.

## Reporting a vulnerability

If you discover a security vulnerability in Advanced Repo Spine, please report it responsibly.

**Do not open a public GitHub issue for security vulnerabilities.**

<!-- TODO: Replace with actual contact before GitHub publication -->
Instead, please email: **[MAINTAINER_EMAIL_TBD]**

Include the following in your report:

- Description of the vulnerability
- Steps to reproduce
- Affected version(s)
- Any potential impact assessment

## Response expectations

- You will receive an acknowledgment within **48 hours**
- A fix timeline will be communicated within **7 days**
- You will be credited in the release notes (unless you prefer otherwise)

## Scope

ARS is a local CLI tool that reads filesystem contents and a JSON model file. Security concerns most relevant to this project include:

- Path traversal in model file processing
- Denial of service via malformed model files
- Unexpected file access outside the intended working directory
- Dependency vulnerabilities in NuGet packages

Issues related to the JSON model format or CLI argument handling are in scope. Issues requiring physical access to the machine running ARS are generally out of scope.
