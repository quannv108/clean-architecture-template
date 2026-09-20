---
type: Mechanism
title: CI Pipeline
description: GitHub Actions builds the solution, runs every test including architecture tests, and publishes code coverage on each push and pull request.
resource: .github/workflows/build-test-coverage.yml
tags: [ci, github-actions, coverage, security-scan]
status: stable
---

# CI Pipeline

## Workflows

| Workflow | Runs |
|---|---|
| `.github/workflows/build-test-coverage.yml` | Build, all tests, coverage report and PR summary |
| `.github/workflows/security-scan.yml` | Dependency and code security scanning |
| `.github/dependabot.yml` | Dependency update pull requests |

Both workflows run on every push and pull request. Pull requests target `main`.

## Coverage

* Report for `main`: https://quannv108.github.io/clean-architecture-template/
* Each pull request gets a coverage summary in its checks.
* Reports are downloadable from the Actions tab.

Reproduce locally with `./scripts/ci-local.sh` (Linux/macOS) or `scripts\ci-local.bat` (Windows), or run the
commands by hand - see [Generate a Coverage Report](../../workflows/engineering/generate-coverage.md).

## What fails the build

Anything that fails `dotnet test CleanArchitecture.slnx`, which includes
[architecture tests](test-architecture.md). Treat a failing architecture test as a design
error, not a test to adjust - the rule it asserts is documented in
[Constraints](../../engineering/constraints.md) with its reasoning.

## Formatting

`dotnet format ... --verify-no-changes --severity error` is the style gate. Fix violations only in files you
created or modified; the solution may carry pre-existing violations elsewhere. See
[Format Code](../../workflows/engineering/format-code.md).
