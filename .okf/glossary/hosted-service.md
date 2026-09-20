---
type: Term
title: "Hosted Service"
description: "A long-running background component started and stopped with the application host."
tags: [async, background-jobs, hosting]
status: stable
---

# Hosted Service

`IHostedService` / `BackgroundService`. They live in **Infrastructure** and are registered in
`Infrastructure/DependencyInjection.cs` - never in Web.Api, which would tie background processing to the
presence of an HTTP host.

Shipped example: [`OutboxMessageHostedService`](../../src/Infrastructure/Outbox/OutboxMessageHostedService.cs).

See [Background Processing](../architecture/cross-cutting/background-processing.md).
