---
type: Mechanism
title: Test Architecture
description: Three test projects with three different jobs - architecture rules, Application unit tests, and end-to-end API integration tests.
resource: tests
tags: [testing, architecture-tests, unit-tests, integration-tests]
status: stable
---

# Test Architecture

| Project | Asks | Tools |
|---|---|---|
| [`ArchitectureTests`](../../../tests/ArchitectureTests) | Is the code shaped correctly? | [NetArchTest](../../engineering/technologies/netarchtest.md) |
| [`Application.UnitTests`](../../../tests/Application.UnitTests) | Does this handler behave? | [NSubstitute](../../engineering/technologies/nsubstitute.md), [Shouldly](../../engineering/technologies/shouldly.md), MockQueryable |
| [`Api.IntegrationTests`](../../../tests/Api.IntegrationTests) | Does the system work end to end? | `WebApplicationFactory`, [Testcontainers](../../engineering/technologies/testcontainers.md) |

## Architecture tests are the gate

`dotnet test tests/ArchitectureTests/` must pass **before any work is considered complete**. They enforce
layer dependencies, visibility, naming, error-class placement, handler shape and testing standards - the
whole of [Constraints](../../engineering/constraints.md).

| Test class | Enforces |
|---|---|
| `Layers/LayerTests.cs` | [Layered Architecture](../cross-cutting/layered-architecture.md) |
| `Domain/DomainTests.cs`, `DomainEventTests.cs` | [Constraints](../../engineering/constraints.md); events are records implementing `IDomainEvent` with the suffix |
| `Application/*HandlerTests.cs`, `CachedRepositoryTests.cs`, `RepositoryTests.cs` | Handler and repository visibility, naming, return types; cached repositories return DTOs |
| `Infrastructure/InfrastructureTests.cs`, `Presentation/PresentationTests.cs` | [Visibility](../../engineering/conventions/visibility.md); endpoints implement `IEndpoint` |
| `CodeQuality/CodeQualityTests.cs` | Sealed classes, no public mutable statics |
| `Testing/TestingStandardsTests.cs` | NSubstitute and Shouldly, not Moq and FluentAssertions |

`BaseTest.cs` holds the assembly references the rules are written against. **A failing architecture test is a
design error, not a test to loosen** - change the decision first ([ADR](../../adr/index.md)), then the test
and the code together.

## Unit tests

* AAA - Arrange, Act, Assert.
* **NSubstitute**, not Moq. **Shouldly**, not FluentAssertions. Enforced by
  `tests/ArchitectureTests/Testing/TestingStandardsTests.cs`.
* Mock `DbSet` with `BuildMock()` from MockQueryable.NSubstitute.
* Named `<Operation>HandlerTests.cs` under `tests/Application.UnitTests/<Feature>/`.
* Target: 70%+ coverage of the Application layer.
* Assert on the `Result`: that a failure carries the expected `Error.Code`, not just that it failed.
* Substitute `IDateTimeProvider` rather than working around `DateTime.UtcNow`, and `IUserContext` rather than
  constructing claims.

## Integration tests

* **Go through API endpoints only. Never write to the database directly.** A test that seeds through the
  DbContext proves nothing about the endpoint, and drifts the moment validation changes. Enforced socially
  and by review; it is the loudest rule in this repository.
* Use [`ApiTestFactory`](../../../tests/Api.IntegrationTests/Infrastructure/ApiTestFactory.cs) (WebApplicationFactory +
  Testcontainers PostgreSQL) and the `ApiClient` helper for authenticated calls. Container startup dominates
  the first test, so the factory is shared per test collection, not per test.
* Remember the `/api/v1` prefix - see [API Surface](../cross-cutting/api-surface.md).
* Call `WaitForOutboxMessagesAsync()` before asserting anything a domain event handler does, because
  [dispatch is asynchronous](../cross-cutting/domain-event-dispatch.md).

## Development workflow

Class enhancements follow TDD: adjust or write the test first, implement, then confirm the whole suite
passes. See [Build and Test](../../workflows/engineering/build-and-test.md).
