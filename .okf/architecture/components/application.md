---
type: Component
title: Application Layer
description: Use cases - CQRS command and query handlers, the abstractions Infrastructure implements, options shapes, cached repositories and job logic.
resource: src/Application
tags: [layer, application, cqrs, use-cases]
status: stable
---

# Application Layer

`src/Application` references Domain and SharedKernel. It is where a use case lives: one handler, one
transaction, one outcome.

## What lives here

| Folder | Holds |
|---|---|
| `Abstractions/Messaging/` | [`ICommand`](../../../src/Application/Abstractions/Messaging/ICommand.cs), [`ICommandHandler`](../../../src/Application/Abstractions/Messaging/ICommandHandler.cs), [`IQuery`](../../../src/Application/Abstractions/Messaging/IQuery.cs), [`IQueryHandler`](../../../src/Application/Abstractions/Messaging/IQueryHandler.cs) |
| `Abstractions/Behaviors/` | The four decorators - [Decorator Pipeline](../cross-cutting/decorator-pipeline.md) |
| `Abstractions/<Concern>/` | Interface contracts Infrastructure implements, plus their `Options` classes |
| `<Feature>/` | `<Operation>CommandHandler.cs`, `<Operation>QueryHandler.cs`, job logic classes, permissions constants |
| `<Feature>/Data/` | [Cached repositories](../../../src/Application) - read side only |
| `<Feature>/Events/` | [`IDomainEventHandler<T>`](../../../src/Application/Abstractions/Messaging/IQueryHandler.cs) implementations |

## The rules that define this layer

* **Handlers are `internal sealed`** and return `Task<Result>` or `Task<Result<T>>`.
* **The command or query record lives in the same file as its handler.** One file, one use case.
* **Commands and queries are `sealed record` with standard (non-positional) syntax** and DataAnnotations;
  [`ValidationDecorator`](../../../src/Application/Abstractions/Behaviors/ValidationDecorator.cs) runs them before the handler. See
  [Record Syntax](../../engineering/conventions/record-syntax.md).
* **Reads go through a cached repository and return DTOs**, never domain entities.
  **Writes inject [`IApplicationDbContext`](../../../src/Application/Abstractions/Data/IApplicationDbContext.cs)** directly. Never the
  reverse. See [Data Access](../cross-cutting/data-access.md).
* **Configuration is shaped here, bound in Infrastructure.** `Application/Abstractions/<Concern>/XOptions.cs`
  declares what the layer needs; Infrastructure decides where it is loaded from. Use `IOptions<T>`, never
  `IConfiguration`. See [Options Pattern](../../engineering/patterns/options-pattern.md).
* **Job logic lives here; job runners do not.** A `*BackgroundJob` class that does *not* implement
  [`IBackgroundJob`](../../../src/Application/Abstractions/BackgroundJobs/IBackgroundJob.cs) is job logic and belongs in
  `Application/<Feature>/`. See [Background Processing](../cross-cutting/background-processing.md).

## Registration

`Application/DependencyInjection.cs` scans the assembly with [Scrutor](../../engineering/technologies/scrutor.md), registers
every handler by its closed interface, and wraps each one in the decorator pipeline. There is no mediator
and no `Send()`. See [CQRS](../cross-cutting/cqrs.md).

## Related

* [CQRS](../cross-cutting/cqrs.md)
* [Options Pattern](../../engineering/patterns/options-pattern.md)
