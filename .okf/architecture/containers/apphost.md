---
type: Container
title: "AppHost (development orchestrator)"
description: "The .NET Aspire host that starts the API together with Postgres, pgweb and Seq for local development. Not deployed."
resource: src/AppHost/AppHost.cs
tags: [container, c4, aspire, local-development]
status: stable
---

# AppHost (development orchestrator)

A [.NET Aspire](../../engineering/technologies/dotnet-aspire.md) application host. One command starts the whole local
stack and wires the connection strings between its parts:

```bash
dotnet run --project src/AppHost
```

| Starts | |
|---|---|
| `db` | [PostgreSQL](postgres.md) |
| `pgweb` | [pgweb](pgweb.md) |
| `seq` | [Seq](seq.md) |
| `Web.Api` | [the application](web-api.md) |

## It is development-time only

AppHost describes a **local process topology**. It is not deployed, and deployed environments get their
configuration from the platform instead. Nothing in `src/Application` or `src/Domain` may reference it, and
it carries no business logic.

It is listed among the containers because it is a process that runs — but it is the one container that
disappears entirely outside a developer's machine.

## Container runtime

`AppHost.cs` sets `DOTNET_ASPIRE_CONTAINER_RUNTIME` to **Podman** before the builder is created, so no
per-developer setup is required. Override the environment variable to use Docker. See
[Local Development Environment](../delivery/local-development-environment.md) and
[ADR 0011: Podman as the default container runtime](../../adr/0011-podman-default-container-runtime.md).

**Aspire's runtime setting does not reach
[Testcontainers](../../engineering/technologies/testcontainers.md)**, which the integration tests start themselves —
see [Run Integration Tests](../../workflows/engineering/run-integration-tests.md).
