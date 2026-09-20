---
type: Technology
title: "Hangfire"
description: "The background job server behind the IBackgroundJob adapter and recurring schedules."
resource: https://www.hangfire.io
tags: [technology, background-jobs, scheduling]
status: stable
---

# Hangfire

Wrapped by [`HangfireBackgroundJob`](../../../src/Infrastructure/BackgroundJobs/Hangfire/HangfireBackgroundJob.cs), with schedules registered in
[`HangfireRecurringJobConfigurator`](../../../src/Infrastructure/BackgroundJobs/Hangfire/HangfireRecurringJobConfigurator.cs) and a dashboard
mounted from `Web.Api/Extensions/Hangfire/`.

**The Hangfire type never leaves Infrastructure.** Application code depends on
[`IBackgroundJob`](../../../src/Application/Abstractions/BackgroundJobs/IBackgroundJob.cs), and the job logic itself lives in
`Application/<Feature>/` with no reference to Hangfire at all - see
[Background Processing](../../architecture/cross-cutting/background-processing.md).

Recurring jobs run on **every** instance, so job logic should take a distributed lock with `TimeSpan.Zero`
and skip when it loses.

Note that the [Outbox](../patterns/outbox-pattern.md) does **not** use Hangfire - it has its own hosted
service, so event delivery does not depend on a job server being healthy.
