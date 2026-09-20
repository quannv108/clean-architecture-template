---
type: Term
title: "Job Logic"
description: "The class that does the work of a background job, with no dependency on the job runner."
tags: [async, background-jobs, placement]
status: stable
---

# Job Logic

An `ExecuteAsync` class in `Application/<Feature>/` that calls command handlers and knows nothing about
Hangfire.

**The naming trap:** a class ending `BackgroundJob` that does *not* implement
[`IBackgroundJob`](../../src/Application/Abstractions/BackgroundJobs/IBackgroundJob.cs) is job logic and belongs in **Application**. Only SDK
adapters implement that interface, and only those live in `Infrastructure/BackgroundJobs/`.

Examples: [`OutboxMessageCleanupJob`](../../src/Application/Outbox/OutboxMessageCleanupJob.cs),
[`DeleteOldAuditLogsBackgroundJob`](../../src/Application/AuditLogs/DeleteOldAuditLogsBackgroundJob.cs).
