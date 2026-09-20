---
type: Workflow
title: "Add an EF Core Migration"
description: "The migration command with its two mandatory flags, and the visibility change that follows every time."
tags: [workflow, ef-core, migrations, database]
status: stable
---

# Add an EF Core Migration

## Add

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure \
  --startup-project src/Web.Api \
  --output-dir Database/Migrations \
  --context ApplicationDbContext \
  -- --environment Migration
```

**Both of these are required, and both fail confusingly when omitted:**

* `--output-dir Database/Migrations` - without it EF generates into `src/Infrastructure/Migrations/`, the
  wrong path and the wrong namespace, and the migrations are not discovered at runtime.
* `-- --environment Migration` - without it the startup project boots in the default environment and may
  fail on missing configuration or services.

## Then change the visibility

**Change `public partial` to `internal partial` on both the migration class and its `.Designer.cs`.** EF
scaffolds them public; `tests/ArchitectureTests` requires every Infrastructure type to be internal. This
step is needed every single time.

## Verify

```bash
dotnet build CleanArchitecture.slnx
dotnet test tests/ArchitectureTests/
```

## Remove

```bash
dotnet ef migrations remove \
  --project src/Infrastructure \
  --startup-project src/Web.Api \
  --context ApplicationDbContext \
  -- --environment Migration
```

**Never delete a migration file by hand.** `migrations remove` also reverts
`ApplicationDbContextModelSnapshot.cs`; deleting the file leaves the snapshot out of sync, and the next
migration you generate produces an incorrect diff - usually discovered much later, against data.

Related: [Persistence](../../architecture/cross-cutting/persistence.md),
[Visibility](../../engineering/conventions/visibility.md).
