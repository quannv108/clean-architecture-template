---
type: Mechanism
title: Vertical Slice Architecture
description: Organising code by business capability across every layer, so one feature is one set of folders with the same name.
tags: [architecture, vertical-slice, structure]
status: stable
---

# Vertical Slice Architecture

Layers answer *what may depend on what*. Slices answer *what belongs together*. A feature called `Orders`
appears as a folder named `Orders` in every layer that needs it:

```
src/Domain/Orders/            Order.cs, OrderErrors.cs, OrderPlacedDomainEvent.cs
src/Application/Orders/       PlaceOrderCommandHandler.cs, Data/, Events/, OrdersPermissionsConstants.cs
src/Infrastructure/Database/Configuration/Orders/   OrderConfiguration.cs
src/Web.Api/Endpoints/Orders/ PlaceOrder.cs, GetOrderById.cs
tests/Application.UnitTests/Orders/
tests/Api.IntegrationTests/Orders/
```

Adding a feature means adding folders, not editing shared files. Deleting one means deleting folders.

## Complexity tiers

Slices scale rather than being one size. See [Add a Feature](../../workflows/engineering/add-a-feature.md)
for the file lists.

| Tier | Operations | Shape | Template examples |
|---|---|---|---|
| **Simple** | 1-2 | Entity + errors, two handlers, one EF config, two endpoints (~6 files) | Profiles, Tenants, AuditLogs |
| **Medium** | 3-5 | Adds related entities, domain events, a cached repository, a seeder (~13-19 files) | Roles, Notifications |
| **Complex** | 6+ | Adds sub-area folders inside Application and Endpoints (~20+ files) | Users, Authentication, Emails |

## Cross-slice communication

Slices are allowed to talk, but only through three doors:

| Mechanism | Use for | Example |
|---|---|---|
| [Domain events](domain-event-dispatch.md) | Asynchronous, decoupled reaction | `UserRegisteredDomainEvent` |
| [Cached repository](../../../src/Application) | Reading another slice's data | Orders reads users via `IUserCachedRepository` |
| Shared response DTOs | Public read shapes | `UserResponse` |

**Anti-patterns** - all of these create the coupling slices exist to prevent:

* Handler calling another slice's handler directly
* Slice A mutating slice B's entities
* A domain entity shared by two slices

If two slices need the same *value object*, it moves to `SharedKernel/<Concept>/`, not into one of them.
See [SharedKernel Layer](../components/shared-kernel.md).

## Related

* [Naming and Placement](../../engineering/conventions/naming.md)
* [Naming](../../engineering/conventions/naming.md)
* [Add a Feature](../../workflows/engineering/add-a-feature.md)
