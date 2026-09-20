---
type: Constraint
title: "Constraints"
description: "Every rule the code must keep, what enforces it, and the page that owns the reasoning - re-check before you finish."
tags: [architecture-tests, rules, review]
status: stable
---

# Constraints

The rules below are the ones that keep the architecture true after the people who set it up have moved on.
`dotnet test tests/ArchitectureTests/` must pass **before any work is considered complete**, including
changes that look unrelated to structure - most violations are a file added in the wrong place.

**A failing architecture test is a design error, not a test to loosen.** If you believe a rule is wrong,
change the decision first (write or supersede an [ADR](../adr/index.md)), then change the test and the code
together. Relaxing a test alone leaves this page claiming something untrue.

Each rule is explained once, on its owner page. This table only says what the rule is and what catches a
violation; "review" means nothing automated does.

| Rule | Enforced by | Owner |
|---|---|---|
| Layer dependencies point inward only: SharedKernel <- Domain <- Application <- Infrastructure <- Web.Api | `Layers/LayerTests.cs` | [Layered Architecture](../architecture/cross-cutting/layered-architecture.md) |
| Domain references only SharedKernel; entities have a private constructor and a static `Create` returning `Result<T>`; events are records implementing `IDomainEvent` with the `DomainEvent` suffix | `Domain/DomainTests.cs`, `Domain/DomainEventTests.cs` | [Domain Layer](../architecture/components/domain.md), [Entity Factory Method](patterns/entity-factory-method.md) |
| No mediator: endpoints inject `ICommandHandler` / `IQueryHandler` directly; MediatR is not referenced | `Application/CommandHandlerTests.cs`, `QueryHandlerTests.cs` assert handler shape; the absence of the package is review | [CQRS](../architecture/cross-cutting/cqrs.md) |
| Handlers are `internal sealed`, named `<Operation>CommandHandler` / `QueryHandler`, return `Result` | `Application/*HandlerTests.cs` | [Command Handler](patterns/command-handler.md), [Query Handler](patterns/query-handler.md) |
| Cached repositories return DTOs, never entities; named `<Feature>CachedRepository` in `<Feature>/Data/` | `Application/CachedRepositoryTests.cs`, `RepositoryTests.cs` | [Cached Read](patterns/cached-read.md) |
| Infrastructure types, handlers and endpoints are `internal`; migrations and their `.Designer.cs` are `internal partial` | `Infrastructure/InfrastructureTests.cs`, `Presentation/PresentationTests.cs`, `CodeQuality/CodeQualityTests.cs` | [Visibility](conventions/visibility.md) |
| Every endpoint implements `IEndpoint` and declares its full OpenAPI contract | `Presentation/PresentationTests.cs` (structure); the builder chain is review | [Minimal API Endpoint](patterns/minimal-api-endpoint.md) |
| `*Errors` classes live in Domain or SharedKernel, never Application or Web.Api | `tests/ArchitectureTests` | [Error Codes](conventions/error-codes.md) |
| Tests use NSubstitute and Shouldly, not Moq and FluentAssertions | `Testing/TestingStandardsTests.cs` | [Test Architecture](../architecture/delivery/test-architecture.md) |
| Configuration is read through `IOptions<T>`, never `IConfiguration` | review - a NetArchTest rule on constructor parameters would close the gap | [Options Pattern](patterns/options-pattern.md) |
| No `ExecuteUpdate` / `ExecuteDelete` in command handlers | review - the failure is silent at runtime, which is what makes it worth a rule | [Data Access](../architecture/cross-cutting/data-access.md) |
| Hosted services live and are registered in Infrastructure, never Web.Api | review - `InfrastructureTests.cs` checks visibility only | [Background Processing](../architecture/cross-cutting/background-processing.md) |
| Integration tests go through API endpoints, never the database | review | [Test Architecture](../architecture/delivery/test-architecture.md) |
| A feature is complete only when every layer's file exists | review - a half-built slice is not visibly broken | [Add a Feature](../workflows/engineering/add-a-feature.md) |

`BaseTest.cs` in `tests/ArchitectureTests/` holds the assembly references the rules are written against;
CI runs the suite on every push and pull request - [CI Pipeline](../architecture/delivery/ci-pipeline.md).
