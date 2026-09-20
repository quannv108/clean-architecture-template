---
type: Mechanism
title: Observability
description: Structured logging with Serilog into Seq, OpenTelemetry traces and metrics, and health checks.
tags: [observability, logging, tracing, serilog, seq, opentelemetry, health-checks]
status: stable
---

# Observability

## Logging

[Serilog](../../engineering/technologies/serilog.md) with structured properties, shipped to
[Seq](../../engineering/technologies/seq.md) (`http://localhost:8081` by default in the Aspire stack).

* [`RequestContextLoggingMiddleware`](../../../src/Web.Api/Middleware/RequestContextLoggingMiddleware.cs) pushes
  correlation properties so every log line in a request shares them.
* [`LoggingDecorator`](../../../src/Application/Abstractions/Behaviors/LoggingDecorator.cs) logs the start, completion and failure of every
  handler. Do not log the same thing inside a handler.
* Log **structured properties**, never interpolated strings: `logger.LogInformation("Email {Id} sent", id)`.
* Never log secrets, tokens or personal data. Lock names and cache keys may be logged, which is why neither
  may embed sensitive values - see [Distributed Lock](../../engineering/patterns/distributed-lock.md).

`CA1873` is currently suppressed in several files because of a .NET 10 analyzer regression - see
[Remove the CA1873 suppressions once the analyzer is fixed](../../backlog/remove-ca1873-suppressions.md).

## Tracing and metrics

[OpenTelemetry](../../engineering/technologies/opentelemetry.md) is wired by
[ServiceDefaults](../components/service-defaults.md).
[`OpenTelemetryInstrumentDecorator`](../../../src/Application/Abstractions/Behaviors/OpenTelemetryInstrumentDecorator.cs) opens an
`Activity` span per handler, and [`ActivityEnricher`](../../../src/Web.Api/ActivityEnricher.cs) adds request
attributes to the ambient span.

## Health checks

`/health` aggregates the registered checks - PostgreSQL always, Redis when configured. `/alive` is the
liveness probe. Both come from [ServiceDefaults](../components/service-defaults.md).

## What is worth watching

| Signal | Why |
|---|---|
| Unprocessed [`OutboxMessage`](../../domains/outbox/outbox-message.md) count and age | Domain events silently not firing |
| Outbox failure count by type | A handler failing repeatedly |
| Cache hit rate | Target above 80% |
| Lock contention and skip rate | Jobs starving each other |
| 412 responses | Optimistic concurrency conflicts - clients should retry |
