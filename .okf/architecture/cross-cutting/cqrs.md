---
type: Mechanism
title: CQRS
description: Commands and queries as first-class types with dedicated handlers, resolved by Scrutor and injected directly - no mediator.
resource: src/Application/Abstractions/Messaging
tags: [cqrs, commands, queries, handlers, scrutor]
status: stable
---

# CQRS

Every use case is a `record` (the command or query) plus a handler. Writes and reads are separate types with
separate data paths.

## The interfaces

| Interface | Returns | For |
|---|---|---|
| [`ICommandHandler<TCommand>`](../../../src/Application/Abstractions/Messaging/ICommandHandler.cs) | `Task<Result>` | A write with no payload |
| [`ICommandHandler<TCommand, TResponse>`](../../../src/Application/Abstractions/Messaging/ICommandHandler.cs) | `Task<Result<TResponse>>` | A write returning something (usually an id) |
| [`IQueryHandler<TQuery, TResponse>`](../../../src/Application/Abstractions/Messaging/IQueryHandler.cs) | `Task<Result<TResponse>>` | A read |
| [`IDomainEventHandler<TEvent>`](../../../src/Application/Abstractions/Messaging/IQueryHandler.cs) | `Task` | A reaction to a dispatched domain event |

All four live in `src/Application/Abstractions/Messaging/`.

## There is no mediator

Handlers are registered by [Scrutor](../../engineering/technologies/scrutor.md) assembly scanning in
`Application/DependencyInjection.cs` and **injected directly** into the endpoint that needs them. There is
no `IMediator`, no `Send()`, no `IRequest`.

```csharp
app.MapPost("/users", async (
    CreateUserCommand command,
    ICommandHandler<CreateUserCommand, Guid> handler,
    CancellationToken ct) =>
{
    Result<Guid> result = await handler.Handle(command, ct);
    return result.Match(Results.Ok, CustomResults.Problem);
});
```

The benefit is that the dependency is visible in the signature: you can see from the endpoint exactly which
use case it invokes, and the compiler catches a missing registration. Reasoning in
[ADR 0002: No MediatR - handlers injected directly](../../adr/0002-no-mediatr.md).

## Shape rules

* The command or query record is declared in the **same file** as its handler.
* Commands and queries are `sealed record` with DataAnnotations, in the syntax
  [Record Syntax](../../engineering/conventions/record-syntax.md) prescribes.
* Handlers are `internal sealed` and never throw for an expected failure; they return
  `Result.Failure(...)`.
* Command handlers write through [`IApplicationDbContext`](../../../src/Application/Abstractions/Data/IApplicationDbContext.cs); query
  handlers read through a [cached repository](../../../src/Application) or a projection. See
  [Data Access](data-access.md).

## Porting from a MediatR codebase

`await _mediator.Send(new CreateUserCommand { ... })` becomes an injected
`ICommandHandler<CreateUserCommand, Guid>` and `await handler.Handle(command, ct)`. Pipeline behaviours become
decorators registered in `Application/DependencyInjection.cs` - later registration means further out. No test
asserts the absence of the MediatR package; a reintroduced reference is caught in review.

## What wraps a handler

Every resolved handler is wrapped by [the decorator pipeline](decorator-pipeline.md) before it reaches the
caller, so logging, validation, concurrency translation and tracing are never written by hand.

## Related

* [Command Handler](../../engineering/patterns/command-handler.md)
* [Query Handler](../../engineering/patterns/query-handler.md)
