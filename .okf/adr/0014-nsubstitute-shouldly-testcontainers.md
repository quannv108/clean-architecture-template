---
type: ADR
title: "ADR 0014: NSubstitute, Shouldly and Testcontainers as the test stack"
description: "Standardise on one mocking library, one assertion library and real containerised dependencies, enforced by architecture tests."
tags: [adr, testing, nsubstitute, shouldly, testcontainers]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0014: NSubstitute, Shouldly and Testcontainers as the test stack

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

Mixed test stacks are a slow tax. Two mocking libraries means every test file starts with a decision and
every reviewer context-switches. The same goes for assertions. And integration tests against in-memory
providers pass while the real database rejects the same query.

## Decision

| Concern | Choice | Not |
|---|---|---|
| Mocking | [NSubstitute](../engineering/technologies/nsubstitute.md) | Moq |
| Assertions | [Shouldly](../engineering/technologies/shouldly.md) | FluentAssertions |
| `DbSet` mocking | MockQueryable.NSubstitute `BuildMock()` | hand-rolled fakes |
| Integration database | [Testcontainers](../engineering/technologies/testcontainers.md) PostgreSQL | EF in-memory / SQLite |
| Architecture rules | [NetArchTest](../engineering/technologies/netarchtest.md) | review |

`tests/ArchitectureTests/Testing/TestingStandardsTests.cs` asserts the first two, so a stray Moq reference
fails the build.

## Consequences

**Good.** One way to write a test. NSubstitute's syntax is lighter for the substitution-heavy style this
architecture produces. Testcontainers means integration tests exercise real PostgreSQL behaviour -
`xmin` concurrency, query filters, string-stored enums - none of which an in-memory provider reproduces.

**Costly.** Testcontainers needs a container runtime and makes integration tests slow to start; under
Podman it needs `DOCKER_HOST` and `TESTCONTAINERS_RYUK_DISABLED`. Developers who know Moq or
FluentAssertions have to switch. NSubstitute's implicit syntax can produce confusing failures when a
substitute is misconfigured.

Also part of this decision: **integration tests go through API endpoints only, never writing to the database
directly** - see [Api.IntegrationTests](../architecture/delivery/test-architecture.md).
