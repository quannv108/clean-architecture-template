---
type: Convention
title: "Visibility"
description: "What is public, what is internal, and why the boundary is enforced by tests."
tags: [visibility, encapsulation, architecture-tests]
status: stable
---

# Visibility

| Type | Visibility |
|---|---|
| Infrastructure services | `internal sealed` |
| DbContexts | `internal` |
| Command / query / domain event handlers | `internal sealed` |
| Endpoints | `internal sealed` |
| EF migrations and their `.Designer.cs` | `internal partial` |
| Entities, value objects, domain events | `public` |
| Domain errors | `public static` factory methods |
| Application abstractions and options | `public` |
| Extension classes, constants, enums | `public` |

## Why

An implementation type that is public will eventually be referenced from a layer that should not know it
exists, and then it cannot be changed. Making it internal means the compiler enforces the boundary rather
than a reviewer.

`sealed` is the default for the same kind of reason - it removes accidental inheritance and lets the JIT
devirtualise calls.

## The migration gotcha

EF Core scaffolds migrations as `public`. **Change both the migration class and its `.Designer.cs` to
`internal partial`**, every time. The architecture tests catch it, which is the only reason anyone
remembers. See [Add an EF Core Migration](../../workflows/engineering/add-ef-migration.md).

Enforced by `tests/ArchitectureTests/Infrastructure/InfrastructureTests.cs` and
`Presentation/PresentationTests.cs` -
[Visibility](visibility.md).
