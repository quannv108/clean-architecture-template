---
type: Workflow
title: "Add a Feature"
description: "The end-to-end procedure for adding a vertical slice, scaled to Simple, Medium or Complex."
tags: [workflow, feature, slice, procedure]
status: stable
---

# Add a Feature

The files are created by hand - there is no scaffolding tool (older docs mentioned a `tools/CodeGenerator`; it
never shipped and is not planned).

## 0. Pick the tier

| Tier | Operations | Files | Examples |
|---|---|---|---|
| **Simple** | 1-2 basic CRUD | ~6 | Profiles, Tenants |
| **Medium** | 3-5 + business logic | ~13-19 | Roles, Notifications |
| **Complex** | 6+, sub-areas | ~20+ | Users, Authentication |

### Simple

```
Domain/<Feature>/           <Feature>.cs, <Feature>Errors.cs
Application/<Feature>/      Create<Feature>Command.cs, Get<Feature>Query.cs,
                            <Feature>PermissionsConstants.cs
Infrastructure/Database/Configuration/<Feature>/   <Feature>Configuration.cs
Web.Api/Endpoints/<Feature>/                       Create<Feature>.cs, Get<Feature>.cs
```

### Medium - adds

```
Domain/<Feature>/           <Related>.cs, <Event>DomainEvent.cs
Application/<Feature>/      Update/Delete handlers
                            Data/I<Feature>CachedRepository.cs
                            Events/<Event>DomainEventHandler.cs
Infrastructure/Database/Configuration/<Feature>/   <Related>Configuration.cs
Infrastructure/Database/Seeder/<Feature>/          <Feature>Seeder.cs
Web.Api/Endpoints/<Feature>/                       Update/Delete endpoints
```

### Complex - adds

Sub-area folders inside `Application/<Feature>/` and `Web.Api/Endpoints/<Feature>/`, each with its own
handlers, repository and endpoints.

## 1. Domain

1. `Domain/<Feature>/<Feature>.cs` inheriting [`Entity`](../../../src/SharedKernel/Entity.cs) or
   [`AuditedEntity`](../../../src/SharedKernel/AuditedEntity.cs).
2. Private constructor, `public static Create(...)` returning `Result<T>` -
   [Entity Factory Method](../../engineering/patterns/entity-factory-method.md).
3. `<Feature>Errors.cs` - `public static` factories, codes `"{Entity}.{ErrorName}"`.
4. Domain events as positional records - [Domain Event](../../engineering/patterns/domain-event.md).
5. Related entities and slice-local value objects.

## 2. Application

1. One file per operation: `<Operation>Command.cs` / `<Operation>Query.cs`, **holding both the record and
   its handler** -
   [Command Handler](../../engineering/patterns/command-handler.md),
   [Query Handler](../../engineering/patterns/query-handler.md).
2. `Data/I<Feature>CachedRepository.cs` for cached reads -
   [Cached Read](../../engineering/patterns/cached-read.md).
3. `Events/` handlers for domain events the slice raises.
4. `<Feature>PermissionsConstants.cs` if secured.

## 3. Infrastructure

1. `Database/Configuration/<Feature>/<Feature>Configuration.cs` - mapping, indexes, relationships.
2. `DbSet<T>` on **both** [`IApplicationDbContext`](../../../src/Application/Abstractions/Data/IApplicationDbContext.cs) and
   [`ApplicationDbContext`](../../../src/Infrastructure/Database/ApplicationDbContext.cs).
3. Seeder under `Database/Seeder/<Feature>/` if needed, **registered in
   [`DbSeeder`](../../../src/Infrastructure/Database/Seeding/DbSeeder.cs)**.
4. Migration - [Add an EF Core Migration](add-ef-migration.md).

## 4. Web.Api

One file per endpoint, following
[Minimal API Endpoint](../../engineering/patterns/minimal-api-endpoint.md). Add the tag constant to
[`Tags`](../../../src/Web.Api/Endpoints/Tags.cs). Mark security-relevant endpoints `.WithAuditLog("...")`.

## 5. Tests

`tests/Application.UnitTests/<Feature>/<Operation>HandlerTests.cs` and
`tests/Api.IntegrationTests/<Feature>/`. Integration tests go **through the API only**, include the
`/api/v1` prefix, and call `WaitForOutboxMessagesAsync()` before asserting event side effects.

## 6. Knowledge

Same commit as the code. Create the slice's folder in this bundle:

```
.okf/domains/<slice>/
  <slice>.md          from ../domains/_templates/domain.md   (type: Domain Slice)
  index.md            a listing: `# Concepts` with <slice>.md first, then the files below
  <entity>.md         from ../domains/_templates/entity.md   (one per entity)
  <value-object>.md   from ../domains/_templates/value-object.md
  <entity>-errors.md  the error factory
```

`index.md` is a listing only, never a concept (OKF reserves it). Then add the slice to
[Domains](../../domains/index.md) and a line to [Knowledge Base Update Log](../../log.md).

## 7. Verify

```bash
dotnet build CleanArchitecture.slnx
dotnet test tests/ArchitectureTests/
dotnet test CleanArchitecture.slnx
dotnet format CleanArchitecture.slnx style --verify-no-changes --severity error
```

Then walk steps 1-6 again as a checklist. A half-built slice is not visibly broken: the endpoint works, so it
passes review, and the missing integration test, tag constant or cache invalidation surfaces weeks later in
someone else's work. "It works" and "it is finished" are different states here.
