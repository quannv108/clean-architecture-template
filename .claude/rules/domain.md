---
paths:
  - "src/Domain/**"
  - "src/SharedKernel/**"
---

# Domain & SharedKernel Rules

Detail: [docs/Architecture.md](../../docs/Architecture.md), [docs/VerticalSliceStructure.md](../../docs/VerticalSliceStructure.md)

## Layer boundaries

- Domain references **only SharedKernel** — no EF, logging, or infrastructure dependencies (enforced by ArchitectureTests).
- SharedKernel has no dependencies at all.

## Entities

- Inherit `Entity` or `AuditedEntity`.
- Private constructor + `public static Create(...)` factory returning `Result<T>`; validation lives in the factory.
- Never set IDs manually — `EntityIdGenerationInterceptor` assigns `Guid.CreateVersion7()`.
- Feature entity file: `Domain/<Feature>/<Feature>.cs`.

## Errors

- `Domain/<Feature>/<Feature>Errors.cs`: static class with `public static` factory methods returning `Error`.
- Error code pattern: `"{Entity}.{ErrorName}"` (e.g. `"User.NotFound"`).
- `*Errors` classes never live in Application or Web.Api (enforced by ArchitectureTests). Cross-cutting errors with no domain entity → `SharedKernel/<Concept>/`.

## Domain events

- `Domain/<Feature>/<Event>DomainEvent.cs` implementing `IDomainEvent`.
- Raise via `entity.Raise(...)`; dispatched **asynchronously via Outbox**, not in-memory — see [docs/DomainEvent.md](../../docs/DomainEvent.md) and [docs/OutboxPattern.md](../../docs/OutboxPattern.md).

## SharedKernel placement

- Root level: DDD primitives only (`Entity`, `ValueObject`, `Result`, `Error`, `IDomainEvent`, `EncryptedString`).
- `SharedKernel/<Concept>/`: value objects shared by **more than one** slice (e.g. `PhoneNumbers/`).
- Used by one slice only → keep in `Domain/<Feature>/`.
- Infrastructure interface contracts (`IEmailSender`, `IDateTimeProvider`, …) go in `Application/Abstractions/`, **never** SharedKernel.
- Full decision table: [docs/VerticalSliceStructure.md → Decision table](../../docs/VerticalSliceStructure.md#decision-table)
