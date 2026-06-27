# Documentation Index

## Agent Instructions

- [agents.md](../agents.md) — Comprehensive AI agent instructions for working in this codebase

## Architecture & Structure

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
