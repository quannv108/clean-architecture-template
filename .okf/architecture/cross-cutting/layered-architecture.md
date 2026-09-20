---
type: Mechanism
title: Layered Architecture
description: The six layers of the solution and the single direction in which dependencies may point.
resource: tests/ArchitectureTests/Layers/LayerTests.cs
tags: [architecture, layers, dependencies]
status: stable
---

# Layered Architecture

Dependencies point **inward only**. Each layer may reference the layers below it and nothing above.

```
SharedKernel      (no dependencies)
     ^
Domain            (SharedKernel)
     ^
Application       (Domain, SharedKernel)
     ^
Infrastructure    (Application, Domain, SharedKernel)
     ^
Web.Api           (all of the above)
```

`AppHost` and `ServiceDefaults` sit outside this stack: `AppHost` orchestrates processes and depends on
nothing in the business stack; `ServiceDefaults` is referenced by hosted applications.

## Why it is a rule and not a guideline

The direction is asserted by `tests/ArchitectureTests/Layers/LayerTests.cs` using NetArchTest. Adding a
reference the wrong way round fails the test run, not code review. If you need the other direction, you
invert: Application declares the interface, Infrastructure implements it.

## What this buys you

* **Domain is testable with no infrastructure.** It cannot reach a database or an HTTP client because it
  cannot see one.
* **Infrastructure is replaceable.** Application declares an interface
  ([Naming and Placement](../../engineering/conventions/naming.md)); Infrastructure supplies
  an implementation; swapping the implementation touches one project.
* **The compiler answers placement questions.** If a type will not compile where you put it, it belongs
  somewhere else.

## The inversion that makes it work

Application needs to send email, hash a password and read the clock - all infrastructure concerns. It
declares `IEmailSender`, `IHasher` and `IDateTimeProvider` in `Application/Abstractions/` and depends on
those. Infrastructure implements them and registers them in DI. The dependency arrow still points inward
because Infrastructure references Application, not the reverse.

## Related

* [Constraints](../../engineering/constraints.md)
* [Naming and Placement](../../engineering/conventions/naming.md)
* [Vertical Slice Architecture](vertical-slice-architecture.md) - the orthogonal axis
