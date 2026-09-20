---
type: Mechanism
title: Domain Event Dispatch
description: How an event raised on an entity becomes an outbox row and later reaches its handlers - asynchronously, never in-memory.
tags: [domain-events, outbox, async, dispatch]
status: stable
---

# Domain Event Dispatch

```
entity.Raise(new XDomainEvent(id))
   -> collected on the entity
   -> SaveChangesAsync() writes an OutboxMessage row in the SAME transaction
   -> OutboxMessageHostedService polls (or is woken by OutboxSignal)
   -> OutboxMessageProcessor deserializes and dispatches
   -> every IDomainEventHandler<XDomainEvent> runs
   -> the row is marked processed, or the error is recorded
```

Each arrow is a concept: [`Entity.Raise`](../../../src/SharedKernel/Entity.cs),
[`OutboxMessage`](../../domains/outbox/outbox-message.md),
[`OutboxMessageHostedService`](../../../src/Infrastructure/Outbox/OutboxMessageHostedService.cs),
[`OutboxMessageProcessor`](../../../src/Application/Outbox),
[`IDomainEventHandler<T>`](../../../src/Application/Abstractions/Messaging/IQueryHandler.cs).

## Why asynchronous

Dispatching in-memory inside `SaveChangesAsync` couples the caller's latency and success to every
subscriber, and loses events if the process dies between commit and dispatch. Persisting the event in the
business transaction makes delivery at-least-once and independent of the request. Reasoning in
[ADR 0003: Dispatch domain events asynchronously via the Outbox](../../adr/0003-async-domain-events-via-outbox.md).

## Consequences you must design for

* **Handlers run after the response is sent.** Nothing a handler does can influence the HTTP result.
* **Handlers run under the system user context**, not the requesting user's.
* **Delivery is at-least-once.** Handlers must be idempotent.
* **Events must be lightweight.** Carry identifiers, not entity graphs - the entity may have changed by the
  time the handler runs.
* **Ordering is not guaranteed** across different events.

## Writing an event and a handler

Shape and code in [Domain Event](../../engineering/patterns/domain-event.md). Placement:

* Event: `src/Domain/<Feature>/<Event>DomainEvent.cs`, an immutable positional record implementing
  [`IDomainEvent`](../../../src/SharedKernel/IDomainEvent.cs).
* Handler: `src/Application/<Feature>/Events/<Event>DomainEventHandler.cs`, `internal sealed`.

## Testing

Unit-test the handler in isolation. In integration tests, call `WaitForOutboxMessagesAsync()` before
asserting the side effect - see [Run Integration Tests](../../workflows/engineering/run-integration-tests.md).
