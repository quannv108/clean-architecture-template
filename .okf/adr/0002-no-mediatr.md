---
type: ADR
title: "ADR 0002: No MediatR - handlers injected directly"
description: "Register handlers with Scrutor and inject them into endpoints instead of routing calls through a mediator."
tags: [adr, cqrs, mediatr, scrutor, di]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0002: No MediatR - handlers injected directly

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

MediatR is the default reflex for CQRS in .NET. It gives one entry point (`IMediator.Send`) and a pipeline
for cross-cutting behaviour.

It also makes every call site identical. An endpoint that sends `CreateUserCommand` tells you nothing about
what handles it; you find out by searching for the type. A missing registration compiles fine and fails at
runtime. And the library became a paid product for commercial use, which is a supply-chain decision on top
of a design one.

## Decision

No mediator. Handlers implement
[`ICommandHandler<T>`](../../src/Application/Abstractions/Messaging/ICommandHandler.cs) /
[`IQueryHandler<T,R>`](../../src/Application/Abstractions/Messaging/IQueryHandler.cs), are registered by
[Scrutor](../engineering/technologies/scrutor.md) assembly scanning, and are **injected directly** into the endpoints
that use them.

Cross-cutting behaviour comes from [Scrutor decorators](../architecture/cross-cutting/decorator-pipeline.md) instead of
mediator pipeline behaviours.

## Consequences

**Good.** The dependency is in the signature - the endpoint names the handler it needs, navigation works,
and a missing registration fails at startup rather than at request time. No `IMediator.Send()` anywhere, and
one less dependency with licensing to track.

**Costly.** Endpoint signatures are longer. An endpoint that needs several use cases takes several
parameters, which is honest but noisier. Decorator registration order in
`Application/DependencyInjection.cs` is load-bearing and has to be understood.

**Rule:** there is no `IMediator`. Enforced by
[CQRS](../architecture/cross-cutting/cqrs.md).
