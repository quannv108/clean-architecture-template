# Outbox Pattern in Clean Architecture

## Overview

The Outbox pattern is implemented in this solution to guarantee reliable, atomic publishing of integration events and messages as part of business transactions. This prevents lost or duplicated events, especially in distributed systems, by persisting events in the database before dispatching them to external systems.

## Implementation Details

### Where Outbox Is Used
- **Domain Layer**: Domain events are raised as part of aggregate operations.
- **Application Layer**: Command and query handlers orchestrate business logic and may trigger domain events.
- **Infrastructure Layer**: The Outbox mechanism persists events and manages their dispatch.

### Outbox Entity
The Outbox entity is defined in `src/Domain/Outbox/OutboxMessage.cs` and represents a message/event to be published. It includes:
- Unique identifier
- Event type
- Payload (serialized event data)
- Occurred timestamp
- Processed timestamp (nullable)
- Error details (nullable)

### Outbox Workflow
1. **Event Creation**: When a domain event occurs, it is captured and serialized into an OutboxMessage entity.
2. **Atomic Persistence**: The OutboxMessage is saved to the database within the same transaction as the business data, ensuring atomicity.
3. **Background Dispatch**: A background job (see `src/Infrastructure/Outbox/OutboxProcessor.cs`) periodically scans unprocessed OutboxMessages, deserializes them, and publishes to external systems (e.g., message bus, email, etc.).
4. **Mark as Processed**: Successfully dispatched messages are marked with a processed timestamp. Failures are logged with error details for retry or manual intervention.

### Key Classes
- `OutboxMessage` (Domain): Represents the persisted event.
- `IOutboxProcessor` (Infrastructure): Interface for background processing.
- `OutboxProcessor` (Infrastructure): Implementation that reads, publishes, and marks messages.
- `IApplicationDbContext` (Infrastructure): Used for atomic persistence of business data and outbox messages.

### Usage Example
- In a command handler, after a business operation, raise a domain event.
- The event is captured and converted to an OutboxMessage.
- Both business changes and the OutboxMessage are committed in a single transaction.
- The OutboxProcessor picks up the message and publishes it.

### Benefits
- **Atomicity**: Guarantees that business changes and event publishing are consistent.
- **Reliability**: Prevents lost or duplicated events.
- **Resilience**: Handles transient failures with retries and error logging.
- **Decoupling**: Separates business logic from external integrations.

### Best Practices
- Always persist OutboxMessages in the same transaction as business data.
- Use background jobs for dispatching, not synchronous event publishing.
- Monitor and alert on failed OutboxMessages.
- Do not expose Outbox entities directly to API consumers.

## References
- [Microsoft Docs: Reliable Event Publishing with the Outbox Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/outbox)
- [Domain-Driven Design](https://domainlanguage.com/ddd/)

---

This document reflects the current Outbox pattern implementation and usage in your Clean Architecture solution. For technical details or code samples, see the referenced source files in the Domain and Infrastructure layers.
