---
type: Reference
title: Solution Layout
description: The projects, directories and root files that make up the repository.
resource: api/CleanArchitecture.slnx
tags: [structure, solution, repository]
status: stable
---

# Solution Layout

## Projects

| Path | Project | Concept |
|---|---|---|
| `api/src/SharedKernel` | `SharedKernel` | [SharedKernel Layer](components/shared-kernel.md) |
| `api/src/Domain` | `Domain` | [Domain Layer](components/domain.md) |
| `api/src/Application` | `Application` | [Application Layer](components/application.md) |
| `api/src/Infrastructure` | `Infrastructure` | [Infrastructure Layer](components/infrastructure.md) |
| `api/src/Web.Api` | `Web.Api` | [Web.Api Layer](components/web-api.md) |
| `api/src/AppHost` | `AppHost` | [AppHost (development orchestrator)](containers/api/apphost.md) |
| `api/src/ServiceDefaults` | `ServiceDefaults` | [ServiceDefaults](components/service-defaults.md) |
| `api/tests/ArchitectureTests` | architecture rules | [ArchitectureTests](delivery/test-architecture.md) |
| `api/tests/Application.UnitTests` | unit tests | [Application.UnitTests](delivery/test-architecture.md) |
| `api/tests/Api.IntegrationTests` | integration tests | [Api.IntegrationTests](delivery/test-architecture.md) |

## Repository root vs. `api/`

The repository is a monorepo; `api/` holds this .NET backend so its siblings (`apps/*`, `infra/`) can sit
next to it without reshuffling paths - see
[ADR 0016](../adr/0016-monorepo-backend-in-api.md). Everything that is specific to the .NET toolchain lives
under `api/`; everything that spans (or will span) more than one app stays at the root.

| File | Purpose |
|---|---|
| `api/CleanArchitecture.slnx` | XML solution file; the target of every `dotnet build` / `dotnet test` |
| `api/Directory.Build.props` | Shared MSBuild properties for every project |
| `api/Directory.Packages.props` | Central package management - all package versions are pinned here, never in a `.csproj` |
| `api/global.json` | Pins the .NET SDK version (only when the cwd is inside `api/`) |
| `api/.editorconfig` | Style and analyzer severity; `dotnet format` enforces it |
| `api/docker-compose.yml` | Container composition for non-Aspire runs |
| `api/scripts/ci-local.sh` / `.bat` | Runs the CI pipeline locally |
| `AGENTS.md` / `CLAUDE.md` | Minimal agent entry point; both point at this bundle |
| `.github/` | GitHub Actions workflows and Dependabot config, repo-wide |
| `.githooks/pre-commit` | Pre-commit gate, repo-wide |
| `.claude/rules/*.md` | Path-scoped rules Claude Code loads when editing matching files |

## Other applications

Empty placeholders, named `<audience>-<platform>` per [Naming and Placement](../engineering/conventions/naming.md);
each team picks its own stack and documents it in the linked concept.

| Path | Holds | Concept |
|---|---|---|
| `apps/admin-web/` | Admin back-office web app | [Admin Web](containers/admin-web/admin-web.md) |
| `apps/customer-web/` | Customer web app | [Customer Web](containers/customer-web/customer-web.md) |
| `apps/customer-mobile/` | Customer mobile app | [Customer Mobile](containers/customer-mobile/customer-mobile.md) |
| `infra/` | Infrastructure-as-code | [Infrastructure](delivery/infrastructure.md) |

## Other directories

* `reference/` - a read-only reference solution from a real product. Useful as an example of a mature slice,
  but **not** part of the build and **not** authoritative. Rules in this bundle win.
* `.deps-upgrade/` - helper scripts for dependency upgrade runs.
* `bin/`, `obj/` - build output; never edit, never document.

## Adding a project

Add it to `CleanArchitecture.slnx`, declare its package versions in `Directory.Packages.props`, and add a
layer rule for it in `api/tests/ArchitectureTests/Layers/LayerTests.cs` - an undeclared project has no enforced
dependency direction. See [Layered Architecture](cross-cutting/layered-architecture.md).
