---
type: Technology
title: "Podman"
description: "The default container runtime for the local Aspire stack."
resource: https://podman.io
tags: [technology, containers, local-development]
status: stable
---

# Podman

Daemonless, rootless container runtime, and the default here - set in `AppHost.cs` before the builder is
created, so nobody has to configure it.

The machine must be running: `podman machine list`.

Two things that surprise people:

* **Switching between Podman and Docker gives fresh containers and volumes.** Persistent containers and
  named volumes do not migrate, so dev Postgres starts empty.
* **[Testcontainers](testcontainers.md) needs its own configuration** - `DOCKER_HOST` and
  `TESTCONTAINERS_RYUK_DISABLED`, because rootless Ryuk is unreliable.

Docker works instead: set `DOTNET_ASPIRE_CONTAINER_RUNTIME=docker`. See
[ADR 0011: Podman as the default container runtime](../../adr/0011-podman-default-container-runtime.md).
