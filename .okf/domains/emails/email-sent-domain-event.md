---
type: Domain Event
title: "EmailSentDomainEvent"
description: "The template's worked domain event - an immutable positional record carrying only an id."
resource: src/Domain/Emails
tags: [domain, email, domain-events]
status: stable
---

# EmailSentDomainEvent

```csharp
public record EmailSentDomainEvent(Guid EmailMessageId) : IDomainEvent;
```

Everything about the shape is intentional: a positional record (immutable), the `DomainEvent` suffix, and a
single identifier rather than the entity.

Carrying the entity would mean serializing a graph into the [`OutboxMessage`](../outbox/outbox-message.md) payload and
handing the handler a snapshot that may already be stale by the time it runs. Carrying the id means the
handler re-loads exactly what it needs.

Raised by [`EmailMessage.MarkAsSent()`](email-message.md), handled by
[`EmailSentDomainEventHandler`](../../../src/Application/ExampleDomainA/Events/EmailSentDomainEventHandler.cs). Shape guide:
[Domain Event](../../engineering/patterns/domain-event.md).
