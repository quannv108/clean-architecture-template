---
type: Convention
title: "Error Codes"
description: "The {Entity}.{ErrorName} code format and where error factories live."
tags: [errors, naming, conventions]
status: stable
---

# Error Codes

```
"{Entity}.{ErrorName}"
```

`"User.NotFound"`, `"Order.InvalidStatusTransition"`, `"Storage.WriteFailed"`.

Declared as `public static` factory methods in `<Feature>Errors.cs`:

```csharp
public static class OrderErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Order.NotFound", $"Order {id} was not found.");

    public static Error InvalidStatusTransition(OrderStatus from, OrderStatus to) =>
        Error.Conflict("Order.InvalidStatusTransition", $"Cannot move from {from} to {to}.");
}
```

## Rules

* **PascalCase on both sides of the dot.** The entity name matches the entity type.
* **Factory methods, not static fields** - an error usually needs context in its message.
* **Codes are API surface.** Clients branch on them, so treat a rename as a breaking change.
* **Choose the [`ErrorType`](../../../src/SharedKernel/Error.cs) deliberately** - it *is* the HTTP status.
  `Validation` 400, `NotFound` 404, `Conflict` 409, `Problem` 412.
* **Messages are shown to clients.** No internal detail, no stack traces, no identifiers the caller should
  not have.

## Placement

| Error relates to | Goes in |
|---|---|
| A domain entity | `Domain/<Feature>/<Feature>Errors.cs` |
| A cross-cutting concern with no entity | `SharedKernel/<Concept>/<Concept>Errors.cs` |

Never in Application or Web.Api (enforced by `tests/ArchitectureTests`). Error codes are part of the domain's
vocabulary and of the API contract: declared in Application, the same failure gets two codes from two
endpoints that should agree. Shipped examples:
[`StorageErrors`](../../../src/SharedKernel/Storage/StorageErrors.cs),
[`ConcurrencyErrors`](../../../src/SharedKernel/Concurrency/ConcurrencyErrors.cs).
