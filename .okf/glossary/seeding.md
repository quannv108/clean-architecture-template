---
type: Term
title: "Seeding"
description: "Inserting baseline reference data at startup."
tags: [data, database, startup]
status: stable
---

# Seeding

[`IEntitySeeder`](../../src/Infrastructure/Database/Seeding/IEntitySeeder.cs) implementations per feature, run by
[`DbSeeder`](../../src/Infrastructure/Database/Seeding/DbSeeder.cs).

Must be **idempotent** - it runs on every start, including against a database that already has the data. A
new seeder must be registered in `DbSeeder` or it silently does nothing.

Reference data only: roles, permissions, lookup tables. Test fixtures belong in the tests.
