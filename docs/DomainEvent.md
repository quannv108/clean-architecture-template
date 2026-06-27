# Domain Events

Domain events represent something significant that happened within the domain, enabling decoupled reactions across the system. In this template they are **dispatched asynchronously via the Outbox pattern**, not in-memory — see [OutboxPattern.md](OutboxPattern.md) for the dispatch mechanism.

## Key Concepts

- **Decoupling**: parts of the system react to events without tight coupling.
- **Cross-cutting concerns**: logging, notifications, or analytics handled in one place.
- **Reliability**: events are persisted in the same transaction as the entity change and dispatched later (Outbox), so they are never lost.

## Defining a Domain Event

`IDomainEvent` (in `SharedKernel`) is a **marker interface with no members**:

```csharp
public interface IDomainEvent;
```

Domain events are immutable **positional records** ending with the `DomainEvent` suffix (file: `<Event>DomainEvent.cs`). Keep them lightweight — carry only the identifiers/data a handler needs, not whole entities:

```csharp
public record EmailSentDomainEvent(Guid EmailMessageId) : IDomainEvent;
```

## Raising a Domain Event

Events are raised by entities (aggregate roots) inside their behavior methods via the protected `Raise(...)` method on the `Entity` base class. They are collected on the entity and turned into `OutboxMessage` records during `SaveChangesAsync()`:

```csharp
public void MarkAsSent()
{
    if (Status != EmailMessageStatus.Pending)
    {
        return;
    }

    Status = EmailMessageStatus.Sent;
    Raise(new EmailSentDomainEvent(Id));
}
```

## Handling a Domain Event

Handlers implement `IDomainEventHandler<T>` and live in `Application/<Feature>/Events/`. They are `internal sealed` and run asynchronously when the Outbox processor dispatches the event:

```csharp
internal sealed class EmailSentDomainEventHandler(ILogger<EmailSentDomainEventHandler> logger)
    : IDomainEventHandler<EmailSentDomainEvent>
{
    public Task Handle(EmailSentDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Email {Id} sent", domainEvent.EmailMessageId);
        return Task.CompletedTask;
    }
}
```

## Best Practices

- **Keep events lightweight**: include only the data handlers need (often just IDs).
- **Don't mutate state in handlers via the originating entity**: handlers run later, under the system user context.
- **Test handlers in isolation** and assert side effects in integration tests using `WaitForOutboxMessagesAsync()` (see [DevelopmentGuideline.md](DevelopmentGuideline.md#testing-requirements)).

## Related Documentation

- [OutboxPattern.md](OutboxPattern.md) — how events are persisted and dispatched asynchronously
- [VerticalSliceStructure.md](VerticalSliceStructure.md) — where event and handler files live
