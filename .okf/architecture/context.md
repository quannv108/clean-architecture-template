---
type: System Context
title: "System Context"
description: "C4 level 1 - what this system is, who uses it, and which external systems it depends on."
resource: https://github.com/quannv108/clean-architecture-template
tags: [c4, context, system, dotnet, clean-architecture]
status: stable
---

# System Context

## The system

A **.NET 10 HTTP API** built as a Clean Architecture template: Domain-Driven Design, CQRS without a
mediator, and Vertical Slice organisation, with the structural rules enforced by tests rather than by
convention.

It is **one system with one deployed container** — see [Containers (C4 level 2)](containers/index.md).

As shipped it is a template, not a product: the business capability is yours to add. What exists today is
the machinery a real system needs on day one, plus three example slices
([Domains](../domains/index.md)) that demonstrate it.

## Who uses it

| Actor | Interaction |
|---|---|
| **API client** — SPA, mobile app, or another service | HTTPS to `/api/v1/...`, JSON in and out, ProblemDetails on failure |
| **Administrator / auditor** | Queries the [audit trail](../engineering/patterns/audit-logging.md) through the API |
| **Developer** | Runs the stack locally with [AppHost](containers/apphost.md); reads this bundle |
| **AI agent** | Reads this bundle, writes code, runs the [Constraints](../engineering/constraints.md) |

The last row is not decoration. The knowledge base exists because agents are expected readers, and the
[structure](../index.md) is shaped for them as much as for people —
[ADR 0015: Replace docs/ with an OKF knowledge base at .okf/](../adr/0015-adopt-okf-knowledge-base.md).

## What it depends on

| External system | Used for | Required | Absent locally |
|---|---|---|---|
| [PostgreSQL](containers/postgres.md) | All persistent state | **Yes** | Aspire container |
| [Redis](containers/redis.md) | Shared cache tier, distributed locks | No | Falls back to L1 + advisory locks |
| [Amazon SES](../engineering/technologies/aws-ses.md) | Sending email | No | [`DummyEmailSender`](../../src/Infrastructure/Communication/Email/DummyEmailSender.cs) |
| [Twilio](../engineering/technologies/twilio.md) | Sending SMS | No | [`DummySmsSender`](../../src/Infrastructure/Communication/Sms/DummySmsSender.cs) |
| Identity provider | Authenticating callers (JWT bearer) | Depends on deployment | — |
| Telemetry backend | Logs, traces, metrics | No | [Seq](containers/seq.md) |

**Only PostgreSQL is required.** Everything else degrades to a local substitute, so the system runs end to
end with one container and no accounts anywhere. That is a deliberate property of the template: the path a
developer exercises is the same path that runs in production, with different implementations behind the
same interfaces.

## What the system is responsible for

* Accepting and validating requests, and authorising them
* Enforcing business rules in the domain model, returning failures as
  [`Result`](../../src/SharedKernel/Result.cs) values rather than exceptions
* Persisting state atomically, including the intent to publish domain events
  ([the Outbox](../engineering/patterns/outbox-pattern.md))
* Delivering those events to handlers afterwards, at least once
* Recording an [audit trail](../engineering/patterns/audit-logging.md) of actions taken through the API
* Emitting logs, traces and metrics

## What it is deliberately not

* **Not a message broker owner.** Integration with other systems would go through the Outbox to a broker;
  the template ships neither.
* **Not multi-container.** Background work runs inside the API process today — see
  [Containers (C4 level 2)](containers/index.md) for what follows from that and how to split it later.
* **Not an identity provider.** It consumes tokens; it does not issue them.

## What ships in the box

| Capability | Concept |
|---|---|
| Layer dependencies enforced by tests | [Layered Architecture](cross-cutting/layered-architecture.md) |
| CQRS without a mediator | [CQRS](cross-cutting/cqrs.md) |
| Handler decorator pipeline | [Decorator Pipeline](cross-cutting/decorator-pipeline.md) |
| EF Core + PostgreSQL | [Persistence](cross-cutting/persistence.md) |
| Asynchronous domain events | [Outbox Pattern](../engineering/patterns/outbox-pattern.md) |
| Two-tier caching | [Caching Tiers](cross-cutting/caching-tiers.md) |
| Field encryption with key rotation | [Field Encryption and Key Rotation](../engineering/patterns/encryption.md) |
| Distributed locking | [Distributed Lock](../engineering/patterns/distributed-lock.md) |
| Audit logging (4W) | [Audit Logging](../engineering/patterns/audit-logging.md) |
| Architecture / unit / integration tests | [Test Architecture](delivery/test-architecture.md) |
| Build, test and coverage CI | [CI Pipeline](delivery/ci-pipeline.md) |

## What is deliberately absent

* No MediatR — [ADR 0002: No MediatR - handlers injected directly](../adr/0002-no-mediatr.md)
* No in-memory domain event dispatch — [ADR 0003: Dispatch domain events asynchronously via the Outbox](../adr/0003-async-domain-events-via-outbox.md)
* No exceptions for expected failures — [ADR 0008: Result<T> for expected failures, exceptions for the unexpected](../adr/0008-result-pattern-over-exceptions.md)
* No FluentValidation — [ADR 0013: DataAnnotations for shape validation, domain errors for business rules](../adr/0013-dataannotations-validation.md)

## Zoom in

[Containers (C4 level 2)](containers/index.md) → [Components (C4 level 3)](components/index.md) →
the source files each component page links (C4 level 4).
