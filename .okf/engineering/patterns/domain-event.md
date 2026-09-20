---
type: Pattern
title: "Domain Event"
description: "Define, raise and handle a domain event: an immutable record carrying ids, raised in a behaviour method, handled asynchronously."
resource: src/Domain
tags: [domain-events, ddd, async]
status: stable
---

# Domain Event

## 1. Define the event

`src/Domain/<Feature>/<Event>DomainEvent.cs` - an immutable positional record with the `DomainEvent`
suffix:

```csharp
public record OrderConfirmedDomainEvent(Guid OrderId) : IDomainEvent;
```

**Carry identifiers, not entities.** The payload is serialized into an
[`OutboxMessage`](../../domains/outbox/outbox-message.md) and read back later, when the entity may already have
changed. An id keeps the payload small and forces the handler to read current state.

## 2. Raise it in a behaviour method

Guard first (so a repeat is a no-op and raises nothing), mutate, then `Raise(new <Event>(Id))`. Raise from
the entity, never from a handler - the entity is what knows the fact occurred.

```csharp
if (Status != OrderStatus.Draft) return;   // idempotent guard
Status = OrderStatus.Confirmed;
Raise(new OrderConfirmedDomainEvent(Id));
```

## 3. Handle it

`src/Application/<Feature>/Events/<Event>DomainEventHandler.cs` - an `internal sealed class` implementing
`IDomainEventHandler<TEvent>`, injecting whatever the reaction needs (`IApplicationDbContext`, a sender).
Several handlers may subscribe to one event; all run.

Handlers run **after the request that raised the event has returned**, on the outbox processor - so:

* **Be idempotent.** Delivery is at-least-once; a retry after a partial failure calls you again.
* **Do not assume the caller's identity.** Handlers run under the system context.
* **Re-load what you need.** The event carries ids; the entity may have changed since.
* **Do not throw for expected conditions.** A throw marks the outbox message failed and it is retried.

## 4. Test it

Unit-test the handler directly. In integration tests call `WaitForOutboxMessagesAsync()` before asserting
the side effect - see [Run Integration Tests](../../workflows/engineering/run-integration-tests.md).

## Renaming caution

Event type names are stored in existing outbox rows. Renaming or moving an event type breaks deserialization
of messages already written - drain the outbox first, or keep a compatibility mapping.

Worked example: [`EmailSentDomainEvent`](../../domains/emails/email-sent-domain-event.md) and
[its handler](../../../src/Application/ExampleDomainA/Events/EmailSentDomainEventHandler.cs).
