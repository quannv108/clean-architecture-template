The reliable asynchronous domain event delivery slice. Start with the slice concept.

# Concepts

* [outbox](outbox.md) - The slice that makes domain event delivery reliable: events persisted with the business transaction and dispatched afterwards.
* [OutboxMessageErrors](outbox-message-errors.md) - Domain error factories for outbox message failures.
* [OutboxMessage](outbox-message.md) - The persisted form of a raised domain event, written in the same transaction as the business data.
