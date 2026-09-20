---
type: Container
title: "Web.Api Container"
description: "The single deployed application: the HTTP API plus the outbox processor and recurring jobs, all in one process."
resource: src/Web.Api
tags: [container, c4, aspnetcore, deployment]
status: stable
---

# Web.Api Container

The only thing that gets deployed. An ASP.NET Core application on
[.NET 10](../../engineering/technologies/dotnet-10.md), built from `src/Web.Api` and referencing every layer beneath it.

## What runs inside it

| Responsibility | Driven by |
|---|---|
| HTTP API under `/api/v1` | [`EndpointExtensions`](../../../src/Web.Api/Extensions/EndpointExtensions.cs) discovering [`IEndpoint`](../../../src/Web.Api/Endpoints/IEndpoint.cs) implementations |
| Domain event delivery | [`OutboxMessageHostedService`](../../../src/Infrastructure/Outbox/OutboxMessageHostedService.cs), a `BackgroundService` |
| Recurring jobs | [Hangfire](../../engineering/technologies/hangfire.md) server and [`HangfireRecurringJobConfigurator`](../../../src/Infrastructure/BackgroundJobs/Hangfire/HangfireRecurringJobConfigurator.cs) |
| Audit writing | [`AuditLoggingMiddleware`](../../../src/Web.Api/Middleware/AuditLoggingMiddleware.cs), fire-and-forget |
| Health, telemetry, resilience | [`ServiceDefaults`](../components/service-defaults.md) |

**All of it in one process.** There is no separate worker container.

## Dependencies out

| Talks to | For | Required |
|---|---|---|
| [PostgreSQL](postgres.md) | Application data, outbox, audit log, advisory locks | Yes |
| [Redis](redis.md) | L2 cache and distributed locks | No |
| [Amazon SES](../../engineering/technologies/aws-ses.md) | Email | No — [`DummyEmailSender`](../../../src/Infrastructure/Communication/Email/DummyEmailSender.cs) locally |
| [Twilio](../../engineering/technologies/twilio.md) | SMS | No — [`DummySmsSender`](../../../src/Infrastructure/Communication/Sms/DummySmsSender.cs) locally |
| OTLP collector / [Seq](seq.md) | Logs, traces, metrics | No |

Every outbound integration is reached through an interface in `Application/Abstractions/`, so the SDK type
stays inside the Infrastructure component — see
[Naming and Placement](../../engineering/conventions/naming.md).

## Running more than one

Instances are stateless apart from their L1 cache, so horizontal scaling works — with three consequences:

1. **Every instance polls the outbox.** Dispatch is safe because each message is claimed and marked, but
   the polling load multiplies.
2. **Every instance runs the recurring schedule.** Job logic must take a
   [distributed lock](../../engineering/patterns/distributed-lock.md) with `TimeSpan.Zero`.
3. **Cache invalidation is not shared without [Redis](redis.md).**

## Inside it

The layer projects: [Components (C4 level 3)](../components/index.md).
