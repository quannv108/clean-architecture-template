---
type: ADR
title: "ADR 0010: Minimal APIs with a discovered IEndpoint interface"
description: "Use minimal APIs, one class per operation implementing IEndpoint, discovered by assembly scan under a versioned group."
tags: [adr, web-api, minimal-api, endpoints, versioning]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0010: Minimal APIs with a discovered IEndpoint interface

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

MVC controllers group many operations into one class, which grows, accumulates constructor dependencies
that most actions do not use, and makes each action's real dependencies invisible. Plain minimal APIs solve
that but tend to sprawl into one enormous `Program.cs`.

## Decision

One `internal sealed class <Operation> : IEndpoint` per operation, in `Endpoints/<Feature>/<Operation>.cs`,
discovered by assembly scan in
[`EndpointExtensions.MapEndpoints`](../../src/Web.Api/Extensions/EndpointExtensions.cs) and registered inside
`app.MapGroup("api/v1")`.

The route handler is a method reference to a `private static HandleAsync`; request and response types are
positional records in the same file; the OpenAPI contract is declared in the builder chain.

## Consequences

**Good.** One file per operation - routing, contract, request/response and handling all visible at once,
and each endpoint's dependencies are exactly what its `HandleAsync` takes. Adding one touches no shared
file. Versioning applies uniformly because the group is applied centrally.

**Costly.** Discovery is implicit, so **an endpoint class that forgets `IEndpoint` never registers and
produces no error** - the symptom is a 404. The `api/v1` prefix is invisible in the endpoint file, which
regularly causes 404s from tests and clients calling the bare path. The builder chain is verbose and has to
be complete for the generated OpenAPI document to be usable.

**Rules:** [Minimal API Endpoint](../engineering/patterns/minimal-api-endpoint.md), enforced by
`tests/ArchitectureTests/Presentation/PresentationTests.cs`. Shape:
[Minimal API Endpoint](../engineering/patterns/minimal-api-endpoint.md).
