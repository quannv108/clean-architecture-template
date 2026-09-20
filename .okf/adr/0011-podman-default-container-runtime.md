---
type: ADR
title: "ADR 0011: Podman as the default container runtime"
description: "Default Aspire to Podman in code so no per-developer setup is needed, with Docker available by environment variable."
tags: [adr, containers, podman, docker, aspire]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0011: Podman as the default container runtime

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

Aspire needs a container runtime for Postgres, pgweb and Seq. Docker Desktop requires a paid licence for
larger organisations; Podman is a drop-in daemonless alternative. Leaving the choice to each developer means
a per-machine setup step and a class of "works on my machine" problems.

## Decision

`src/AppHost/AppHost.cs` sets `DOTNET_ASPIRE_CONTAINER_RUNTIME` to Podman **before the builder is created**,
so the default is in source control. The environment variable overrides it for anyone who prefers Docker.

## Consequences

**Good.** Cloning and running needs no runtime configuration. No licensing question by default. Docker users
are one environment variable away.

**Costly.** Podman must have a running machine, which is an extra step over Docker Desktop's tray icon and
an unhelpful failure when forgotten. `ContainerLifetime.Persistent` containers and named volumes do not
migrate between runtimes, so switching gives a fresh empty Postgres - expected, but confusing the first
time.

**Important:** [Testcontainers](../engineering/technologies/testcontainers.md) in `Api.IntegrationTests` talks to a
Docker-compatible socket directly and **ignores this setting entirely**. Under Podman it needs `DOCKER_HOST`
and `TESTCONTAINERS_RYUK_DISABLED` - the rootless Ryuk reaper is unreliable there. See
[Run Integration Tests](../workflows/engineering/run-integration-tests.md).
