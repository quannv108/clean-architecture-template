---
type: Technology
title: ".NET Aspire"
description: "The orchestration stack that runs the API together with its dependencies for local development."
resource: https://learn.microsoft.com/dotnet/aspire
tags: [technology, aspire, orchestration, local-development]
status: stable
---

# .NET Aspire

Used by [`AppHost`](../../architecture/containers/apphost.md) to start PostgreSQL, pgweb, Seq and the API together, and by
[`ServiceDefaults`](../../architecture/components/service-defaults.md) for health checks, OpenTelemetry, service discovery and
HTTP resilience.

Aspire is a **development-time orchestrator**, not a deployment target. Deployed environments get their
configuration from the platform; AppHost describes a local topology.

Container runtime defaults to [Podman](podman.md), set in code so no per-developer setup is needed - see
[ADR 0011: Podman as the default container runtime](../../adr/0011-podman-default-container-runtime.md).

**Aspire's runtime setting does not apply to [Testcontainers](testcontainers.md)**, which the integration
tests use.
