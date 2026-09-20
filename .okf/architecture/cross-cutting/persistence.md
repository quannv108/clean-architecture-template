---
type: Mechanism
title: Persistence
description: PostgreSQL through EF Core - contexts, configurations, interceptors, converters, migrations, seeding and schema.
resource: src/Infrastructure/Database
tags: [persistence, ef-core, postgresql, migrations]
status: stable
---

# Persistence

[PostgreSQL](../../engineering/technologies/postgresql.md) via [EF Core](../../engineering/technologies/ef-core.md), all of it in
`src/Infrastructure/Database`.

## Contexts

| Type | Interface | Use |
|---|---|---|
| [`ApplicationDbContext`](../../../src/Infrastructure/Database/ApplicationDbContext.cs) | [`IApplicationDbContext`](../../../src/Application/Abstractions/Data/IApplicationDbContext.cs) | Writes, in command handlers |
| [`ReadOnlyApplicationDbContext`](../../../src/Infrastructure/Database/ReadOnlyApplicationDbContext.cs) | [`IReadOnlyApplicationDbContext`](../../../src/Application/Abstractions/Data/IReadOnlyApplicationDbContext.cs) | Untracked reads |
| [`BaseApplicationDbContext`](../../../src/Infrastructure/Database/BaseApplicationDbContext.cs) | - | Shared conventions both inherit |

The base class exists so the two contexts cannot drift: if they ever disagreed about a filter, queries would
return different rows depending on which one was injected.

Contexts are `internal`. See [Visibility](../../engineering/conventions/visibility.md).

## Conventions applied globally

| Convention | Where |
|---|---|
| Enums stored as strings | [`BaseApplicationDbContext`](../../../src/Infrastructure/Database/BaseApplicationDbContext.cs) |
| Soft-delete query filter on every `Entity` | [Soft Delete](../../engineering/patterns/soft-delete.md) |
| `xmin` mapped to `Entity.Version` as row version | [Optimistic Concurrency](../../engineering/patterns/optimistic-concurrency.md) |
| `EncryptedString` transparently encrypted | [`EncryptedStringConverter`](../../../src/Infrastructure/Database/Converters/EncryptedStringConverter.cs) |
| Version 7 GUID ids assigned on add | [`EntityIdGenerationInterceptor`](../../../src/Infrastructure/Database/Interceptors/EntityIdGenerationInterceptor.cs) |
| Created/modified stamps | [`AuditableEntityInterceptor`](../../../src/Infrastructure/Database/Interceptors/AuditableEntityInterceptor.cs) |

## Per-entity configuration

One `IEntityTypeConfiguration<T>` per entity, at
`Infrastructure/Database/Configuration/<Feature>/<Entity>Configuration.cs`. It owns table and column names,
indexes, relationships and max lengths. Schema names are constants in
[`SchemaNameConstants`](../../../src/Infrastructure/Database/SchemaNameConstants.cs).

## Adding an entity

1. Add `DbSet<T>` to `ApplicationDbContext` **and** `IApplicationDbContext` (and the read-only pair if it is queried).
2. Add `<Entity>Configuration.cs` under `Database/Configuration/<Feature>/`.
3. Add a migration (below).
4. Add a seeder if it needs data, and register it in `DbSeeder`.

## Migrations

In `src/Infrastructure/Database/Migrations`, and **`internal partial`** - architecture tests require every
Infrastructure type to be internal, and EF scaffolds them public. The `--output-dir` and
`-- --environment Migration` flags are both mandatory. Never delete a migration file by hand; use
`dotnet ef migrations remove`, which also reverts the model snapshot. Full procedure:
[Add an EF Core Migration](../../workflows/engineering/add-ef-migration.md).

## Seeding

[`IEntitySeeder`](../../../src/Infrastructure/Database/Seeding/IEntitySeeder.cs) implementations per feature, orchestrated by
[`DbSeeder`](../../../src/Infrastructure/Database/Seeding/DbSeeder.cs). Register a new seeder there or it never runs.

## Deleting

[`IEntityDeleter`](../../../src/Application/Abstractions/Data/IEntityDeleter.cs) /
[`EntityDeleter`](../../../src/Infrastructure/Database/EntityDeleter.cs) perform soft deletes consistently. Do not set `IsDeleted`
by hand and do not `ExecuteDelete` - [Data Access](data-access.md) says why.
