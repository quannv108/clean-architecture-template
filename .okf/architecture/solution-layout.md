---
type: Reference
title: Solution Layout
description: The projects, directories and root files that make up the repository.
resource: CleanArchitecture.slnx
tags: [structure, solution, repository]
status: stable
---

# Solution Layout

## Projects

| Path | Project | Concept |
|---|---|---|
| `src/SharedKernel` | `SharedKernel` | [SharedKernel Layer](components/shared-kernel.md) |
| `src/Domain` | `Domain` | [Domain Layer](components/domain.md) |
| `src/Application` | `Application` | [Application Layer](components/application.md) |
| `src/Infrastructure` | `Infrastructure` | [Infrastructure Layer](components/infrastructure.md) |
| `src/Web.Api` | `Web.Api` | [Web.Api Layer](components/web-api.md) |
| `src/AppHost` | `AppHost` | [AppHost (development orchestrator)](containers/apphost.md) |
| `src/ServiceDefaults` | `ServiceDefaults` | [ServiceDefaults](components/service-defaults.md) |
| `tests/ArchitectureTests` | architecture rules | [ArchitectureTests](delivery/test-architecture.md) |
| `tests/Application.UnitTests` | unit tests | [Application.UnitTests](delivery/test-architecture.md) |
| `tests/Api.IntegrationTests` | integration tests | [Api.IntegrationTests](delivery/test-architecture.md) |

## Root files

| File | Purpose |
|---|---|
| `CleanArchitecture.slnx` | XML solution file; the target of every `dotnet build` / `dotnet test` |
| `Directory.Build.props` | Shared MSBuild properties for every project |
| `Directory.Packages.props` | Central package management - all package versions are pinned here, never in a `.csproj` |
| `global.json` | Pins the .NET SDK version |
| `.editorconfig` | Style and analyzer severity; `dotnet format` enforces it |
| `docker-compose.yml` | Container composition for non-Aspire runs |
| `AGENTS.md` / `CLAUDE.md` | Minimal agent entry point; both point at this bundle |
| `.claude/rules/*.md` | Path-scoped rules Claude Code loads when editing matching files |
| `scripts/ci-local.sh` / `.bat` | Runs the CI pipeline locally |

## Other directories

* `reference/` - a read-only reference solution from a real product. Useful as an example of a mature slice,
  but **not** part of the build and **not** authoritative. Rules in this bundle win.
* `.deps-upgrade/` - helper scripts for dependency upgrade runs.
* `bin/`, `obj/` - build output; never edit, never document.

## Adding a project

Add it to `CleanArchitecture.slnx`, declare its package versions in `Directory.Packages.props`, and add a
layer rule for it in `tests/ArchitectureTests/Layers/LayerTests.cs` - an undeclared project has no enforced
dependency direction. See [Layered Architecture](cross-cutting/layered-architecture.md).
