---
type: Component
title: SharedKernel Layer
description: The dependency-free project holding DDD primitives and cross-slice value objects that every other layer may use.
resource: src/SharedKernel
tags: [layer, sharedkernel, ddd]
status: stable
---

# SharedKernel Layer

`src/SharedKernel` has **no dependencies at all** - not on Domain, not on any package that implies a
runtime. Everything above it may reference it.

It holds exactly two categories of thing.

## Category 1 - DDD building blocks (root level)

Types every layer inherits from, implements, or uses as a property type. No domain meaning, no side effects.

| File | Purpose |
|---|---|
| [`Entity.cs`](../../../src/SharedKernel/Entity.cs) | Base of every entity: `Id` (assigned on save - never set it yourself), `IsDeleted` ([Soft Delete](../../engineering/patterns/soft-delete.md)), `Version` mapped to `xmin` ([Optimistic Concurrency](../../engineering/patterns/optimistic-concurrency.md)), and `protected Raise(IDomainEvent)` which `SaveChangesAsync` turns into outbox rows |
| [`AuditedEntity.cs`](../../../src/SharedKernel/AuditedEntity.cs) | `Entity` plus created/modified stamps, written by the interceptor - inherit it when you need "who last changed this row and when"; the stamps cost four columns. Row provenance, not the [audit log](../../engineering/patterns/audit-logging.md) of API actions |
| [`ValueObject.cs`](../../../src/SharedKernel/ValueObject.cs) | No id; equality from `GetEqualityComponents()`. Validate in a `Create` factory so an instance that exists is valid - [Value Object Template](../../domains/_templates/value-object.md) |
| [`Result.cs`](../../../src/SharedKernel/Result.cs) | `Result` / `Result<T>`: the outcome of anything that can fail, collapsed by `Match` in endpoints - [ADR 0008](../../adr/0008-result-pattern-over-exceptions.md) |
| [`Error.cs`](../../../src/SharedKernel/Error.cs) | `Code`, `Description`, `ErrorType` (which *is* the HTTP status) - [Error Codes](../../engineering/conventions/error-codes.md) |
| [`IDomainEvent.cs`](../../../src/SharedKernel/IDomainEvent.cs) | Marker for immutable event records that carry identifiers, not entities - [Domain Event](../../engineering/patterns/domain-event.md) |
| [`ITenantEntity.cs`](../../../src/SharedKernel/ITenantEntity.cs) | Exposes the tenant id; a global query filter scopes reads, as for soft delete. Single-tenant products simply never implement it |
| [`EncryptedString.cs`](../../../src/SharedKernel/EncryptedString.cs) | Field-level encryption wrapper - [Encryption](../../engineering/patterns/encryption.md) |
| [`EntityIdGenerator.cs`](../../../src/SharedKernel/EntityIdGenerator.cs) | `Guid.CreateVersion7()` so ids sort by creation time and B-tree inserts stay at the right edge. Called by the id interceptor on add - **never assign `Id` in a factory or handler** |
| [`Extensions/`](../../../src/SharedKernel/Extensions) | Assembly scanning, enum, string, string-list and `IQueryable` paging helpers. Only helpers with no business meaning - anything visible to every layer cannot be kept out of the wrong one |

**Rule:** a type belongs here if any layer may use it *and* it carries no domain-specific meaning.

## Category 2 - Cross-slice value objects (named subfolders)

A value object needed by **more than one** domain slice cannot live in `Domain/<Feature>/`, because other
slices would then depend on that feature's folder. Each such concept gets its own subfolder:

```
SharedKernel/
  PhoneNumbers/
    PhoneNumber.cs
    PhoneNumberErrors.cs
  Concurrency/
    ConcurrencyErrors.cs
  Storage/
    StorageErrors.cs
```

Candidates as a project grows: `Money/`, `Address/`, `EmailAddress/`.

**Rule:** more than one slice needs it -> `SharedKernel/<Concept>/`. One slice needs it ->
`Domain/<Feature>/`.

## What must never go here

Infrastructure interface contracts (`IEmailSender`, `IDateTimeProvider`, `IStorage`, ...). They live in
`Application/Abstractions/` so that Domain physically cannot call infrastructure. See
[Naming and Placement](../../engineering/conventions/naming.md).

## Related

* [Naming and Placement](../../engineering/conventions/naming.md) - the full decision table
* [Error Codes](../../engineering/conventions/error-codes.md)
