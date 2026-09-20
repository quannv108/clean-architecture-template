---
paths:
  - "src/Application/**"
---

# Application Layer Rules

Detail: [.okf/architecture/components/application.md](../../.okf/architecture/components/application.md),
[.okf/architecture/cross-cutting/cqrs.md](../../.okf/architecture/cross-cutting/cqrs.md),
[.okf/engineering/patterns/command-handler.md](../../.okf/engineering/patterns/command-handler.md),
[.okf/engineering/patterns/query-handler.md](../../.okf/engineering/patterns/query-handler.md)

## CQRS — no MediatR

- Implement `ICommandHandler<T>` / `ICommandHandler<T,R>` / `IQueryHandler<T,R>` from
  `Application/Abstractions/Messaging/`. Registered via Scrutor; injected directly into endpoints — there is
  no `IMediator.Send()`. See [.okf/architecture/cross-cutting/cqrs.md](../../.okf/architecture/cross-cutting/cqrs.md).
- Handlers: `internal sealed`, return `Task<Result>` or `Task<Result<T>>`.
- Define the command/query record in the **same file** as its handler.
- Commands/Queries: `sealed record`, standard (non-positional) syntax, with DataAnnotations —
  `ValidationDecorator` runs them before the handler.
  See [.okf/engineering/conventions/record-syntax.md](../../.okf/engineering/conventions/record-syntax.md).

## Data access

- **Reads**: cached repository in `Application/<Feature>/Data/`, returns Response DTOs — **never domain
  entities**. See [.okf/engineering/patterns/cached-read.md](../../.okf/engineering/patterns/cached-read.md).
- **Writes**: inject `IApplicationDbContext` directly in command handlers — never a cached repository.
- Never `ExecuteUpdate`/`ExecuteDelete` — bypasses domain events, change tracking and the Outbox.
  See [.okf/architecture/cross-cutting/data-access.md](../../.okf/architecture/cross-cutting/data-access.md).
- Pattern: 1 load → N in-memory mutations → 1 `SaveChangesAsync()`.
  See [.okf/engineering/patterns/atomic-transaction.md](../../.okf/engineering/patterns/atomic-transaction.md).

## Placement

- Infrastructure interface contracts and their Options classes go in `Application/Abstractions/`; use
  `IOptions<T>`, never `IConfiguration`.
  See [.okf/engineering/conventions/naming.md](../../.okf/engineering/conventions/naming.md).
- Job logic classes (`*BackgroundJob` that do **not** implement `IBackgroundJob`) live in
  `Application/<Feature>/`, not Infrastructure.
  See [.okf/architecture/cross-cutting/background-processing.md](../../.okf/architecture/cross-cutting/background-processing.md).
- Domain event handlers (`IDomainEventHandler<T>`) go in `Application/<Feature>/Events/`.
- Permissions: `Application/<Feature>/<Feature>PermissionsConstants.cs`.
