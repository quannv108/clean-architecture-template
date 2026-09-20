---
type: Mechanism
title: Local Development Environment
description: Running the full stack locally with .NET Aspire, and the Podman/Docker container runtime choice.
tags: [local-development, aspire, podman, docker, containers]
status: stable
---

# Local Development Environment

## Prerequisites

* [.NET 10 SDK](../../engineering/technologies/dotnet-10.md) - pinned by `global.json`.
* [Podman](../../engineering/technologies/podman.md) with a running machine (`podman machine list`), or Docker.

## Running everything

```bash
dotnet run --project src/AppHost
```

[AppHost](../containers/apphost.md) starts PostgreSQL, pgweb, Seq and the API together and injects the connection strings.

## Container runtime

Aspire defaults to **Podman**: `AppHost.cs` sets `DOTNET_ASPIRE_CONTAINER_RUNTIME` before the builder is
created, so no per-developer configuration is needed. Override it to use Docker:

```powershell
$env:DOTNET_ASPIRE_CONTAINER_RUNTIME='docker'; dotnet run --project src/AppHost
```

In VS Code, add it to the `env` block of the "Run Aspire AppHost" launch configuration.

**Persistence caveat:** `ContainerLifetime.Persistent` containers and named volumes created under Docker do
not carry over to Podman or back. Switching runtimes gives you empty containers and volumes, so dev Postgres
starts empty and needs migrations and seed again. This is expected.

## Integration tests are separate

[Testcontainers](../../engineering/technologies/testcontainers.md) talks to a Docker-compatible socket directly and ignores
the Aspire runtime setting. Under Podman it needs `DOCKER_HOST` and `TESTCONTAINERS_RYUK_DISABLED` - see
[Run Integration Tests](../../workflows/engineering/run-integration-tests.md).

## Useful local endpoints

| URL | What |
|---|---|
| `http://localhost:8081` | Seq log viewer |
| `/health`, `/alive` | Health and liveness |
| `/scalar` or the OpenAPI UI | Generated API documentation |
| Dev pages under `/Dev` | [Email and outbox inspectors](../../../src/Web.Api/Pages/Dev) |

Port numbers other than Seq's are assigned by Aspire; read them from the Aspire dashboard rather than
hard-coding them.
