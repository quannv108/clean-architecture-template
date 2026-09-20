Recurring implementation shapes, each with copyable code and a checklist. Read the one that matches what you
are about to write.

# Writing a use case

* [Command Handler](command-handler.md) - The shape of a write use case: command record and handler in one file, load, mutate, save once, return a Result.
* [Query Handler](query-handler.md) - The shape of a read use case: query record and handler in one file, projection to a DTO, no writes.
* [Minimal API Endpoint](minimal-api-endpoint.md) - The required shape of an endpoint: IEndpoint, a static HandleAsync, positional request records and a complete builder chain.
* [Entity Factory Method](entity-factory-method.md) - Private constructor plus a static Create returning Result<T>, so an entity that exists is always valid.

# Data

* [Atomic Transaction](atomic-transaction.md) - One load, N in-memory mutations, one save - and when an explicit transaction is actually needed.
* [Optimistic Concurrency](optimistic-concurrency.md) - Detect concurrent entity writes with the PostgreSQL xmin row version, and return 412 for the client to retry.
* [Cached Read](cached-read.md) - Put HybridCache in front of a projection, return DTOs, and invalidate after every write that changes the data.
* [Soft Delete](soft-delete.md) - Mark rows deleted instead of removing them, hidden by a model-wide query filter.

# Asynchronous work

* [Outbox Pattern](outbox-pattern.md) - Persist a raised domain event in the same transaction as the business change, and dispatch it from a background service afterwards.
* [Domain Event](domain-event.md) - Define, raise and handle a domain event: an immutable record carrying ids, raised in a behaviour method, handled asynchronously.
* [Distributed Lock](distributed-lock.md) - Coordinate work across application instances with a named lock - and the cases where you should not use one.

# Cross-cutting

* [Options Pattern](options-pattern.md) - Application declares the configuration it needs as an Options class beside its abstraction; Infrastructure binds and validates it; consumers take IOptions<T>, never IConfiguration.
* [Field Encryption and Key Rotation](encryption.md) - Encrypt a column by declaring EncryptedString, and rotate keys by configuration without a data migration.
* [Audit Logging](audit-logging.md) - Record who did what, when and where by marking an endpoint - non-intrusive, asynchronous, and never able to fail a request.

# Template

* [Pattern Template](_template-pattern.md) - Copy this when documenting a recurring implementation shape.
