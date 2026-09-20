---
type: Template
title: "Entity Template"
description: "Copy this when adding an entity or aggregate root."
tags: [template, domain, entity]
status: stable
---

# Entity Template

> Copy to `.okf/domains/<slice>/<entity>.md`, replace everything, add it to [Domains](../index.md). Delete this quote
> block.

```yaml
---
type: Entity
title: "Order"
description: "One sentence: what this entity represents in the business."
resource: src/Domain/Orders/Order.cs
tags: [domain, orders, entity]
status: draft
---
```

## What it represents

A business concept, in business words. Not "a row in the Orders table".

## State

| Property | Type | Meaning |
|---|---|---|

Say which properties are set once at creation, which change through behaviour, and which are derived.

## Behaviour

One subsection per behaviour method: what it does, what it guards against, what it raises.

```csharp
public void Confirm()
{
    if (Status != OrderStatus.Draft) return;          // guard makes it idempotent
    Status = OrderStatus.Confirmed;
    Raise(new OrderConfirmedDomainEvent(Id));         // carries only required inputs for event, not the whole entity
}
```

## Creation

The `Create(...)` factory: what it validates, and which [`Error`](../../../src/SharedKernel/Error.cs) each failure
returns.

## Invariants

What is always true of a valid instance, and where each is enforced.

## Relationships

Child entities it owns, and the aggregate boundary - what may be loaded and saved with it, and what must be
reached through its own slice instead.

## Checklist

- [ ] Inherits [`Entity`](../../../src/SharedKernel/Entity.cs) or [`AuditedEntity`](../../../src/SharedKernel/AuditedEntity.cs)
- [ ] Private constructor, `public static Create(...)` returning `Result<T>`
- [ ] `Id` never assigned by hand
- [ ] `<Entity>Errors.cs` beside it in `Domain/<Feature>/`
- [ ] `<Entity>Configuration.cs` under `Infrastructure/Database/Configuration/<Feature>/`
- [ ] `DbSet<T>` on both [`IApplicationDbContext`](../../../src/Application/Abstractions/Data/IApplicationDbContext.cs) and the context
- [ ] Migration added - [Add an EF Core Migration](../../workflows/engineering/add-ef-migration.md)
