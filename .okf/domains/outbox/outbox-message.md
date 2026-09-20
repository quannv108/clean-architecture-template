---
type: Entity
title: "OutboxMessage"
description: "The persisted form of a raised domain event, written in the same transaction as the business data."
resource: src/Domain/Outbox/OutboxMessage.cs
tags: [domain, outbox, entity, domain-events]
status: stable
---

# OutboxMessage

| Property | Meaning |
|---|---|
| `Id` | Identifier |
| Event type | The concrete [`IDomainEvent`](../../../src/SharedKernel/IDomainEvent.cs) type name, used to deserialize |
| Payload | The serialized event |
| Occurred at | When the event was raised |
| Processed at | Null until dispatched successfully |
| Error | The failure detail when dispatch failed |

Created during `SaveChangesAsync` from the events an entity [raised](../../../src/SharedKernel/Entity.cs), in the same
transaction as the business change.

## Rules

* **Never construct or write one by hand.** `Raise` the event on the entity; the save path does the rest.
* **Never expose it through the API.** It is an internal delivery mechanism, not a resource; the
  [dev pages](../../../src/Web.Api/Pages/Dev) are for developers and are development-only.
* A row with a processed timestamp is done; a row with an error still needs attention; a row with neither is
  waiting.

Consumed by [`OutboxMessageProcessor`](../../../src/Application/Outbox); mapped and indexed by
[`OutboxMessageConfiguration`](../../../src/Infrastructure/Database/Configuration/Outbox/OutboxMessageConfiguration.cs); pruned once processed by
[`CleanupProcessedOutboxMessagesCommand`](../../../src/Application/Outbox/CleanupProcessedOutboxMessagesCommand.cs).
