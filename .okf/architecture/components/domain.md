---
type: Component
title: Domain Layer
description: Pure business logic - entities, domain events, domain errors and slice-local value objects, referencing only SharedKernel.
resource: src/Domain
tags: [layer, domain, ddd]
status: stable
---

# Domain Layer

`src/Domain` references **only SharedKernel**. No EF Core, no logging, no HTTP, no clock. Enforced by
`tests/ArchitectureTests/Domain/DomainTests.cs`.

## What lives here

Organised per slice as `Domain/<Feature>/`:

* `<Feature>.cs` - the aggregate root entity, inheriting [`Entity`](../../../src/SharedKernel/Entity.cs) or
  [`AuditedEntity`](../../../src/SharedKernel/AuditedEntity.cs)
* `<Feature>Errors.cs` - a static class of `public static` factory methods returning
  [`Error`](../../../src/SharedKernel/Error.cs)
* `<Event>DomainEvent.cs` - immutable positional records implementing
  [`IDomainEvent`](../../../src/SharedKernel/IDomainEvent.cs)
* `<Related>.cs` - child entities and slice-local value objects

Slices that ship with the template: [Audit Logs](../../domains/audit-logs/audit-logs.md),
[Emails](../../domains/emails/emails.md), [Outbox](../../domains/outbox/outbox.md).

## Rules that make it "pure"

* **Construction goes through a factory.** Private constructor plus `public static Create(...)` returning
  `Result<T>`; validation lives in the factory, not in the caller. See
  [Entity Factory Method](../../engineering/patterns/entity-factory-method.md).
* **IDs are never set by hand.** [`EntityIdGenerationInterceptor`](../../../src/Infrastructure/Database/Interceptors/EntityIdGenerationInterceptor.cs)
  assigns `Guid.CreateVersion7()` on save.
* **State changes happen in behaviour methods**, and those methods `Raise(...)` domain events. See
  [Domain Event Dispatch](../cross-cutting/domain-event-dispatch.md).
* **Failure is a return value.** Domain code returns `Result.Failure(SomeErrors.X())`; it does not throw for
  expected outcomes. See [ADR 0008: Result<T> for expected failures, exceptions for the unexpected](../../adr/0008-result-pattern-over-exceptions.md).
* **No time, no randomness, no I/O.** If a behaviour needs "now", the Application layer passes it in via
  [`IDateTimeProvider`](../../../src/Application/Abstractions/Time/IDateTimeProvider.cs).

## Error codes

`"{Entity}.{ErrorName}"`, e.g. `"User.NotFound"`, `"Order.InvalidStatusTransition"`. See
[Error Codes](../../engineering/conventions/error-codes.md).

## Related

* [Constraints](../../engineering/constraints.md)
* [Entity Template](../../domains/_templates/entity.md)
