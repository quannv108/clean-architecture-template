---
type: Pattern
title: "Outbox Pattern"
description: "Persist a raised domain event in the same transaction as the business change, and dispatch it from a background service afterwards."
resource: src/Application/Outbox
tags: [outbox, domain-events, reliability, async]
status: stable
---

# Outbox Pattern

## Problem

A command changes state and something else must happen as a result - send mail, notify a partner, update a
projection. Doing it inline makes the caller wait for it and fail with it. Doing it after the commit, in
memory, loses it if the process dies in between.

## Solution

Write the intent to publish into the database **inside the business transaction**, and deliver it
separately.

```
entity.Raise(new OrderConfirmedDomainEvent(order.Id));
await db.SaveChangesAsync(ct);     // order row + OutboxMessage row, one transaction
```

| Step | Owner |
|---|---|
| Collect raised events | [`Entity`](../../../src/SharedKernel/Entity.cs) |
| Write [`OutboxMessage`](../../domains/outbox/outbox-message.md) rows | [`ApplicationDbContext`](../../../src/Infrastructure/Database/ApplicationDbContext.cs) save path |
| Poll (or be woken by [`OutboxSignal`](../../../src/Infrastructure/Outbox/OutboxSignal.cs)) | [`OutboxMessageHostedService`](../../../src/Infrastructure/Outbox/OutboxMessageHostedService.cs) |
| Deserialize, dispatch, mark processed or failed | [`OutboxMessageProcessor`](../../../src/Application/Outbox) - in Application, not Infrastructure, so it unit-tests without a host |
| Resolve and invoke handlers | [`DomainEventsDispatcher`](../../../src/Infrastructure/DomainEvents/DomainEventsDispatcher.cs) |
| Prune processed rows | [`OutboxMessageCleanupJob`](../../../src/Application/Outbox/OutboxMessageCleanupJob.cs) |

## Rules for handlers

* **Idempotent.** Delivery is at-least-once; a retry after a partial failure calls you again.
* **Re-load what you need.** The event carries ids; state may have moved on.
* **Do not throw for expected conditions.** A throw marks the message failed and it is retried forever.
* **Do not assume a caller.** Handlers run under the system identity.

## Operating it

Watch pending count and pending age. A rising backlog means every asynchronous side effect in the system is
silently not happening - and the only symptom is absence. Failures by type:
[`GetOutboxTypeErrorsQuery`](../../../src/Application/Outbox/GetOutboxTypeErrorsQuery.cs) and the
[dev pages](../../../src/Web.Api/Pages/Dev).

## Related

* [ADR 0003: Dispatch domain events asynchronously via the Outbox](../../adr/0003-async-domain-events-via-outbox.md)
* [Domain Event](domain-event.md) - writing the event and handler
* [Backlog](../../backlog/index.md) - known gaps in the current implementation
