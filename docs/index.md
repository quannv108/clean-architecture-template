# Documentation Index

## Agent Instructions

- [CLAUDE.md](../CLAUDE.md) — Concise entry point: critical rules, quick commands, documentation map
- [Development Guideline](DevelopmentGuideline.md) — Branching, build/test/format commands, testing requirements, EF migrations, common pitfalls, temporary suppressions

## Architecture & Structure

- [Architecture](Architecture.md) — Layers, CQRS/decorator pipeline, data access, domain events, code & naming conventions, error codes
- [Vertical Slice Structure](VerticalSliceStructure.md) — Feature organization, SharedKernel placement rules, naming conventions
- [Feature Templates](FeatureTemplates.md) — Templates for Simple/Medium/Complex features

## Patterns

- [Domain Event](DomainEvent.md) — Domain event implementation
- [Outbox Pattern](OutboxPattern.md) — Async domain event dispatch via Outbox
- [Caching](Caching.md) — HybridCache patterns for read-side queries
- [Encryption](Encryption.md) — Encrypted data storage with EncryptedString
- [Concurrency](Concurrency.md) — Optimistic concurrency, atomic transactions, and distributed locks
- [Distributed Lock](DistributedLock.md) — Distributed locking (PostgreSQL/Redis)
- [Audit Logging](AuditLogging.md) — Audit logging (4W: Who, What, When, Where)
