---
paths:
  - "src/Application/**"
---

# Application Layer Rules

Detail: [docs/Architecture.md → CQRS](../../docs/Architecture.md#cqrs-scrutor-decorators-not-mediatr), [docs/VerticalSliceStructure.md](../../docs/VerticalSliceStructure.md), [docs/FeatureTemplates.md](../../docs/FeatureTemplates.md)

## CQRS — no MediatR

- Implement `ICommandHandler<T>` / `ICommandHandler<T,R>` / `IQueryHandler<T,R>` from `Application/Abstractions/Messaging/`. Registered via Scrutor; injected directly into endpoints — there is no `IMediator.Send()`.
- Handlers: `internal sealed`, return `Task<Result>` or `Task<Result<T>>`.
- Define the command/query record in the **same file** as its handler.
- Commands/Queries: `sealed record`, standard (non-positional) syntax, with DataAnnotations — `ValidationDecorator` runs them before the handler.

## Data access

- **Reads**: CachedRepository in `Application/<Feature>/Data/`, returns Response DTOs — **never domain entities**. See [docs/Caching.md](../../docs/Caching.md).
- **Writes**: inject `IApplicationDbContext` directly in command handlers — never use CachedRepository there.
- Never `ExecuteUpdate`/`ExecuteDelete` — bypasses domain events, change tracking, and the Outbox.
- Pattern: 1 load (all data upfront) → N in-memory mutations → 1 `SaveChangesAsync()`. See [docs/Concurrency.md](../../docs/Concurrency.md).

## Placement

- Infrastructure interface contracts and their Options classes go in `Application/Abstractions/`; use `IOptions<T>`, never `IConfiguration`.
- Job logic classes (`*BackgroundJob` that do **not** implement `IBackgroundJob`) live in `Application/<Feature>/`, not Infrastructure.
- Domain event handlers (`IDomainEventHandler<T>`) go in `Application/<Feature>/Events/`.
- Permissions: `Application/<Feature>/<Feature>PermissionsConstants.cs`.
