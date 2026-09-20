---
type: ADR
title: "ADR 0001: Clean Architecture with Vertical Slices"
description: "Combine inward-pointing layer dependencies with per-capability slices, and enforce the layering with tests rather than convention."
tags: [adr, architecture, layers, vertical-slice]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0001: Clean Architecture with Vertical Slices

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

Layered architectures decay when the layering is a convention. Someone adds a package reference in a hurry,
the domain starts knowing about EF Core, and two years later the business rules cannot be tested without a
database.

Organising code strictly by technical role has the opposite failure: every feature is spread across
`Controllers/`, `Services/`, `Repositories/` and `Models/`, so adding one means touching a dozen shared
folders and deleting one is archaeology.

## Decision

Use both axes.

* **Layers** decide what may depend on what: SharedKernel <- Domain <- Application <- Infrastructure <-
  Web.Api, inward only.
* **Slices** decide what belongs together: a feature is a folder of the same name in each layer that needs
  it.
* **The layering is asserted by tests**, not by review - `tests/ArchitectureTests/Layers/LayerTests.cs`.

See [Layered Architecture](../architecture/cross-cutting/layered-architecture.md) and
[Vertical Slice Architecture](../architecture/cross-cutting/vertical-slice-architecture.md).

## Consequences

**Good.** The compiler and the test suite answer most placement questions. Domain is testable with no
infrastructure. Adding a feature means adding folders; removing one means deleting them.

**Costly.** More projects and more ceremony than a single-assembly application - the inversion for every
infrastructure concern (`IEmailSender` in Application, implementation in Infrastructure) is real overhead
on a small system. A team that has not internalised the dependency rule will experience the architecture
tests as friction before it experiences them as help.

**Accepted trade-off:** this is a template for systems expected to live for years and be worked on by
people who did not write them - including agents. Predictable structure is worth the ceremony there. It
would be the wrong choice for a prototype.
