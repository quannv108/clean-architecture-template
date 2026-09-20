---
type: Workflow
title: "Run Integration Tests"
description: "Running the Testcontainers-backed integration tests, including the two environment variables Podman needs."
tags: [workflow, testing, testcontainers, podman, docker]
status: stable
---

# Run Integration Tests

```bash
dotnet test tests/Api.IntegrationTests/
```

## Under Podman

[Testcontainers](../../engineering/technologies/testcontainers.md) talks to a Docker-compatible API socket directly and
**ignores** Aspire's `DOTNET_ASPIRE_CONTAINER_RUNTIME` setting. Two variables are needed:

```powershell
# Point Testcontainers at the Podman machine's API pipe (Windows)
$env:DOCKER_HOST = 'npipe://./pipe/podman-machine-default'

# Podman's rootless Ryuk reaper is unreliable
$env:TESTCONTAINERS_RYUK_DISABLED = 'true'

dotnet test tests/Api.IntegrationTests/
```

* The Podman machine must be running - `podman machine list`.
* The pipe name matches the machine name; confirm with
  `podman machine inspect --format '{{.ConnectionInfo.PodmanPipe.Path}}'`.

## Under Docker

Nothing to set. Testcontainers finds `npipe://./pipe/docker_engine` by default.

## Writing them

* **Through API endpoints only** - never write to the database directly. See
  [Test Architecture](../../architecture/delivery/test-architecture.md).
* Use [`ApiTestFactory`](../../../tests/Api.IntegrationTests/Infrastructure/ApiTestFactory.cs) and the `ApiClient` helper.
* **Include the `/api/v1` prefix** - the bare path from the endpoint file returns 404.
* **Call `WaitForOutboxMessagesAsync()`** before asserting a domain event side effect, or the test races
  the outbox and fails intermittently.
