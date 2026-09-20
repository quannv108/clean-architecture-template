---
type: Workflow
title: "Run the Stack Locally"
description: "Starting Postgres, pgweb, Seq and the API together with Aspire."
tags: [workflow, local-development, aspire, podman]
status: stable
---

# Run the Stack Locally

```bash
dotnet run --project src/AppHost
```

[AppHost](../../architecture/containers/apphost.md) starts PostgreSQL, pgweb, Seq and the API, and injects the connection
strings.

## Prerequisites

* [.NET 10](../../engineering/technologies/dotnet-10.md) SDK — pinned by `global.json`
* [Podman](../../engineering/technologies/podman.md) with a running machine (`podman machine list`), or Docker

## Using Docker instead

```powershell
$env:DOTNET_ASPIRE_CONTAINER_RUNTIME='docker'; dotnet run --project src/AppHost
```

In VS Code, add it to the `env` block of the "Run Aspire AppHost" launch configuration.

---

**Everything else about the local stack** — what each container is for, the ports, the dev pages, and the
caveat that switching runtimes gives you empty volumes — is described once in
[Local Development Environment](../../architecture/delivery/local-development-environment.md).

Integration tests do **not** use this stack; they start their own container. See
[Run Integration Tests](run-integration-tests.md).
