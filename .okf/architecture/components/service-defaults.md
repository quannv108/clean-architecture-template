---
type: Component
title: ServiceDefaults
description: Shared Aspire service wiring - health checks, OpenTelemetry, service discovery and HTTP resilience - applied by every hosted application.
resource: src/ServiceDefaults/Extensions.cs
tags: [aspire, observability, health-checks, resilience]
status: stable
---

# ServiceDefaults

`src/ServiceDefaults` is the standard .NET Aspire service-defaults project. Hosted applications call its
extension methods during startup to pick up, in one line each:

* **OpenTelemetry** - traces, metrics and logs with the standard ASP.NET Core, HttpClient and runtime
  instrumentation. See [Observability](../cross-cutting/observability.md).
* **Health checks** - the `/health` and `/alive` endpoints, including any provider checks registered
  elsewhere (PostgreSQL always; Redis when configured).
* **Service discovery** - resolves logical resource names to endpoints supplied by
  [AppHost](../containers/apphost.md).
* **HTTP resilience** - standard retry, timeout and circuit-breaker handlers on outbound `HttpClient`s.

## When to change it

Only for cross-cutting host behaviour that every application must have. Anything specific to the API -
CORS, rate limits, OpenAPI, endpoint mapping - belongs in `src/Web.Api/Extensions/`, not here.

`ServiceDefaults` must not reference Application, Domain or Infrastructure.
