---
type: Pattern
title: "Command Handler"
description: "The shape of a write use case: command record and handler in one file, load, mutate, save once, return a Result."
tags: [cqrs, commands, handlers, application]
status: stable
---

# Command Handler

A write use case is one file, `src/Application/<Feature>/<Operation>Command.cs`, holding the command record
and its handler. The handler orchestrates; the entity decides.

## How

* The command is a `public sealed record : ICommand` (or `ICommand<TResponse>` when something, usually an
  id, comes back) with DataAnnotations - [Record Syntax](../conventions/record-syntax.md) says which form,
  the [validation decorator](../../architecture/cross-cutting/decorator-pipeline.md) reads them.
* The handler is `internal sealed class <Operation>CommandHandler : ICommandHandler<...>` and injects
  `IApplicationDbContext` - never a cached repository, which is read-side and returns DTOs.
* Body shape: **load -> call a behaviour method -> save once -> return.** Load everything the operation needs
  in one query, let the entity apply the rule, `SaveChangesAsync` exactly once (entity and outbox rows in
  one transaction), return `Result.Success(...)`.
* Expected failures are values: `return Result.Failure(<Feature>Errors.NotFound(id))`. Never throw for
  "not found", "conflict" or "invalid".
* If the change invalidates a cached read, call `RemoveCacheAsync` after the save.

```csharp
var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);
if (order is null) return Result.Failure(OrderErrors.NotFound(command.OrderId));

Result confirm = order.Confirm();          // the rule lives on the entity
if (confirm.IsFailure) return confirm;

await db.SaveChangesAsync(ct);             // once: entity + outbox
return Result.Success();
```

Real instance: [`CreateAuditLogCommand.cs`](../../../src/Application/AuditLogs/CreateAuditLogCommand.cs).

## Checklist

- [ ] `<Operation>Command.cs` holds the `sealed record` and the `internal sealed` handler
- [ ] Writes through [`IApplicationDbContext`](../../../src/Application/Abstractions/Data/IApplicationDbContext.cs)
- [ ] One `SaveChangesAsync`
- [ ] Returns `Result.Failure(<Feature>Errors.X())`, never throws for an expected failure
- [ ] `RemoveCacheAsync` after saving if a cached read is affected
- [ ] No entry/exit logging, manual validation or `DbUpdateConcurrencyException` catch - the
      [decorators](../../architecture/cross-cutting/decorator-pipeline.md) do all three

## The recurring mistake

An `if` about what the domain permits, written in the handler. That rule belongs in a behaviour method on
the entity - otherwise it is unenforced everywhere else the entity is used. See
[Entity Factory Method](entity-factory-method.md).
