---
paths:
  - "src/Domain/**"
  - "src/SharedKernel/**"
---

# Domain & SharedKernel Rules

Detail: [.okf/architecture/components/domain.md](../../.okf/architecture/components/domain.md),
[.okf/architecture/components/shared-kernel.md](../../.okf/architecture/components/shared-kernel.md),
[.okf/engineering/conventions/naming.md](../../.okf/engineering/conventions/naming.md)

## Layer boundaries

- Domain references **only SharedKernel** — no EF, logging, clock or infrastructure dependencies.
  Enforced: [.okf/engineering/constraints.md](../../.okf/engineering/constraints.md).
- SharedKernel has no dependencies at all.

## Entities

- Inherit `Entity` or `AuditedEntity`.
- Private constructor + `public static Create(...)` factory returning `Result<T>`; validation in the factory.
  See [.okf/engineering/patterns/entity-factory-method.md](../../.okf/engineering/patterns/entity-factory-method.md).
- Never set IDs manually — `EntityIdGenerationInterceptor` assigns `Guid.CreateVersion7()`.
- Feature entity file: `Domain/<Feature>/<Feature>.cs`.

## Errors

- `Domain/<Feature>/<Feature>Errors.cs`: static class with `public static` factory methods returning `Error`.
- Error code pattern: `"{Entity}.{ErrorName}"`.
  See [.okf/engineering/conventions/error-codes.md](../../.okf/engineering/conventions/error-codes.md).
- `*Errors` classes never live in Application or Web.Api; cross-cutting errors with no domain entity →
  `SharedKernel/<Concept>/`.
  Enforced: [.okf/engineering/conventions/error-codes.md](../../.okf/engineering/conventions/error-codes.md).

## Domain events

- `Domain/<Feature>/<Event>DomainEvent.cs` implementing `IDomainEvent`, carrying identifiers not entities.
- Raise via `entity.Raise(...)`; dispatched **asynchronously via Outbox**, not in-memory.
  See [.okf/engineering/patterns/domain-event.md](../../.okf/engineering/patterns/domain-event.md) and
  [.okf/engineering/patterns/outbox-pattern.md](../../.okf/engineering/patterns/outbox-pattern.md).

## SharedKernel placement

- Root level: DDD primitives only (`Entity`, `ValueObject`, `Result`, `Error`, `IDomainEvent`,
  `EncryptedString`).
- `SharedKernel/<Concept>/`: value objects shared by **more than one** slice (e.g. `PhoneNumbers/`).
- Used by one slice only → keep in `Domain/<Feature>/`.
- Infrastructure interface contracts go in `Application/Abstractions/`, **never** SharedKernel.
- Full decision table:
  [.okf/engineering/conventions/naming.md](../../.okf/engineering/conventions/naming.md).
