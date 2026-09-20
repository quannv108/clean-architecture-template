---
type: Pattern
title: "Entity Factory Method"
description: "Private constructor plus a static Create returning Result<T>, so an entity that exists is always valid."
tags: [ddd, domain, entity, validation]
status: stable
---

# Entity Factory Method

An entity cannot be constructed in an invalid state, because the only way to construct one is a factory
that validates first and returns a `Result<T>`.

## How

* **Private parameterless constructor** (EF Core needs it) and **private setters** - state changes only
  through behaviour methods, which is where domain events are raised.
* `public static Result<T> Create(...)` validates what the entity can see by itself - required values,
  ranges, formats, internal consistency - and returns `Result.Failure<T>(<Feature>Errors.X())` for each rule
  it rejects. Failure is a value the API maps to a status, not an exception.
* **Never assign `Id`.** The id interceptor sets a version-7 GUID on save; setting it yourself defeats that
  contract silently.
* Behaviour methods (`Confirm()`, `Cancel()`) follow the same rule: guard, mutate, `Raise(...)`, return a
  `Result`.

```csharp
private Order() { }

public static Result<Order> Create(Guid customerId, string reference)
{
    if (customerId == Guid.Empty) return Result.Failure<Order>(OrderErrors.CustomerRequired());
    return Result.Success(new Order { CustomerId = customerId, Reference = reference.Trim() });
}
```

Real instance: [`EmailMessage.cs`](../../../src/Domain/Emails/EmailMessage.cs).

## What belongs in the factory, and what does not

Rules that need other data - "this reference is already taken", "this customer is over their credit
limit" - belong in the **command handler**, which can query. A factory that took a DbContext would put
infrastructure in the domain. The same goes for the clock (`IDateTimeProvider` is injected in Application and
passed in) and for logging (return a `Result`; the decorators report it). Domain references only SharedKernel,
and `Domain/DomainTests.cs` asserts the entity shape.

## Related

* [Entity Template](../../domains/_templates/entity.md) - the `.okf` page to write for a new entity
* [Command Handler](command-handler.md) - the caller
* [Domain Event](domain-event.md) - what behaviour methods raise
* [`Result.cs`](../../../src/SharedKernel/Result.cs), [`Error.cs`](../../../src/SharedKernel/Error.cs)
