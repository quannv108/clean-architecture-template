---
type: Mechanism
title: Background Processing
description: Hosted services, recurring jobs and the split between job logic in Application and job-runner adapters in Infrastructure.
tags: [background-jobs, hosted-service, hangfire, outbox]
status: stable
---

# Background Processing

Three distinct things are easy to confuse. They live in different layers on purpose.

| Concept | Where | Examples |
|---|---|---|
| **Hosted service** - a long-running `BackgroundService` owned by the host | `src/Infrastructure/<Feature>/` | [`OutboxMessageHostedService`](../../../src/Infrastructure/Outbox/OutboxMessageHostedService.cs) |
| **Job logic** - the work itself; calls command handlers, knows no job-runner SDK | `src/Application/<Feature>/` | [`OutboxMessageCleanupJob`](../../../src/Application/Outbox/OutboxMessageCleanupJob.cs), [`DeleteOldAuditLogsBackgroundJob`](../../../src/Application/AuditLogs/DeleteOldAuditLogsBackgroundJob.cs) |
| **Job-runner adapter** - implements [`IBackgroundJob`](../../../src/Application/Abstractions/BackgroundJobs/IBackgroundJob.cs), wraps the SDK | `src/Infrastructure/BackgroundJobs/` | [`HangfireBackgroundJob`](../../../src/Infrastructure/BackgroundJobs/Hangfire/HangfireBackgroundJob.cs), [`SimpleBackgroundJob`](../../../src/Infrastructure/BackgroundJobs/SimpleBackgroundJob.cs) |

## The rule that decides placement

A class whose name ends in `BackgroundJob` but that does **not** implement `IBackgroundJob` is job logic and
belongs in `Application/<Feature>/`. It exposes an `ExecuteAsync` method, calls `ICommandHandler<T>`, and has
no reference to Hangfire or any other runner. `Infrastructure/BackgroundJobs/` contains only adapters and
[`HangfireRecurringJobConfigurator`](../../../src/Infrastructure/BackgroundJobs/Hangfire/HangfireRecurringJobConfigurator.cs), which registers
schedules.

## Registration

Hosted services are registered in `Infrastructure/DependencyInjection.cs` with
`services.AddHostedService<T>()`. **Never** in `Web.Api/DependencyInjection.cs` - that ties background
processing to the presence of an HTTP host, so it cannot run as a separate worker. Placement is not asserted
by a test; the architecture tests check visibility only.

## Coordinating across instances

A recurring job that runs on every instance will do its work N times. Take a distributed lock with
`TimeSpan.Zero` so non-winners skip rather than queue. See
[Distributed Lock](../../engineering/patterns/distributed-lock.md).

## Related

* [Outbox Pattern](../../engineering/patterns/outbox-pattern.md)
* [Constraints](../../engineering/constraints.md)
