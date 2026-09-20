---
type: ADR
title: "ADR 0012: Infrastructure and endpoints are internal, enforced by tests"
description: "Keep implementation types internal so the layer boundary is enforced by the compiler rather than by discipline."
tags: [adr, visibility, architecture-tests, encapsulation]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0012: Infrastructure and endpoints are internal, enforced by tests

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

A layer boundary that exists only in documentation gets crossed. If `ApplicationDbContext` is public,
someone will inject it into an endpoint; if a Twilio adapter is public, someone will reference it from
Application. Each such reference makes the implementation impossible to change without a ripple.

## Decision

Infrastructure services, the DbContexts, handlers and endpoints are **`internal sealed`**. Only interfaces,
extension classes, configuration types, constants and enums are public.

`tests/ArchitectureTests/Infrastructure/InfrastructureTests.cs` and
`Presentation/PresentationTests.cs` assert it.

## Consequences

**Good.** The boundary is enforced by the compiler, not by review. Implementations can be replaced freely.
`sealed` also removes accidental inheritance and lets the JIT devirtualise.

**Costly.** Test projects need `InternalsVisibleTo`. EF Core scaffolds migrations as `public`, so **every
generated migration and its `.Designer.cs` must be changed to `internal partial`** - a manual step that is
easy to forget and fails the architecture tests when missed, which is the only reason it is reliably
remembered. Some libraries expect public types and need adapting.

**Rule:** [Visibility](../engineering/conventions/visibility.md). Migration procedure:
[Add an EF Core Migration](../workflows/engineering/add-ef-migration.md).
