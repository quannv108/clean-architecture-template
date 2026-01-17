# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Test Commands

```bash
# Build solution
dotnet build CleanArchitecture.slnx

# Run all tests
dotnet test CleanArchitecture.slnx

# Run with Aspire (full stack with containers)
dotnet run --project src/AppHost

# Create database migration
dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/Web.Api -o Database/Migrations --context ApplicationDbContext

# Local CI pipeline
./scripts/ci-local.sh        # Linux/macOS
scripts\ci-local.bat         # Windows

# Generate code coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:"Html"
```

## Architecture Overview

This is a .NET 10 Clean Architecture template implementing DDD with CQRS and Vertical Slice Architecture.

### Layer Dependencies (enforced by ArchitectureTests)

```
SharedKernel (no dependencies)
     ↑
Domain (only SharedKernel)
     ↑
Application (Domain + SharedKernel)
     ↑
Infrastructure (Application, Domain, SharedKernel)
     ↑
Web.Api (all layers)
```

### Project Structure

- **SharedKernel** - Common DDD abstractions and base classes
- **Domain** - Pure business logic, entities, domain events, value objects
- **Application** - Use cases, CQRS handlers, CachedRepositories
- **Infrastructure** - EF Core, PostgreSQL, authentication, external services
- **Web.Api** - Minimal API endpoints
- **AppHost** - .NET Aspire orchestration

### Key Patterns

**CQRS**: Commands (`ICommand`/`ICommand<T>`) for writes, Queries (`IQuery<T>`) for reads. All handlers return `Task<Result>` or `Task<Result<T>>`.

**Data Access**:
- **Reads**: CachedRepository classes in `Application/<Feature>/Data/` using HybridCache, returning DTOs (never entities)
- **Writes**: Direct `IApplicationDbContext` injection in command handlers

**Vertical Slice**: Features organized by business capability across all layers:
```
Domain/<Feature>/<Feature>.cs, <Feature>Errors.cs
Application/<Feature>/<Operation>CommandHandler.cs, Data/
Infrastructure/<Feature>/<Feature>Configuration.cs
Web.Api/Endpoints/<Feature>/<Operation>.cs
```

## Naming Conventions

| Element | Pattern | Example |
|---------|---------|---------|
| Domain Entity | `<Feature>.cs` | `User.cs` |
| Domain Errors | `<Feature>Errors.cs` | `UserErrors.cs` |
| Domain Event | `<Event>DomainEvent.cs` | `UserRegisteredDomainEvent.cs` |
| Command Handler | `<Operation>CommandHandler.cs` | `CreateRoleCommandHandler.cs` |
| Query Handler | `<Operation>QueryHandler.cs` | `GetUserByIdQueryHandler.cs` |
| CachedRepository | `I<Feature>CachedRepository.cs` | `IUserCachedRepository.cs` |
| EF Configuration | `<Entity>Configuration.cs` | `UserConfiguration.cs` |
| Endpoint | `<Operation>.cs` | `CreateRole.cs` |

## Code Conventions

### Record Syntax
- **Web.Api**: Positional syntax - `public record MyRecord(string Prop1, int Prop2);`
- **Application (Commands/Queries)**: Standard syntax with DataAnnotations for validation:
  ```csharp
  public sealed record CreateUserCommand : ICommand<Guid>
  {
      [Required]
      public string Email { get; init; }
  }
  ```

### Visibility
- Infrastructure services: `internal sealed`
- Handlers: `internal sealed`
- Endpoints: `internal sealed` implementing `IEndpoint`
- Domain errors: `public static` factory methods
- DbContext: `internal`

### Entity Creation
- Private constructors with public static `Create()` factory methods returning `Result<T>`
- Validation in factory methods

## Testing Requirements

- **Unit tests**: 70%+ coverage for Application layer, use NSubstitute (not Moq), use `BuildMock()` from MockQueryable.NSubstitute for DbSet mocking
- **Integration tests**: MUST use API endpoints only - NEVER write directly to database
- **Architecture tests**: Enforce layer dependencies via NetArchTest.Rules
- **Assertions**: Use Shouldly (not FluentAssertions)

## Key Documentation

- `agents.md` - Comprehensive AI agent instructions
- `docs/FeatureTemplates.md` - Templates for Simple/Medium/Complex features
- `docs/VerticalSliceStructure.md` - Feature organization patterns
- `docs/DomainEvent.md` - Domain event implementation
- `docs/Caching.md` - HybridCache patterns

## Common Pitfalls to Avoid

- Using CachedRepository in command handlers (use DbContext directly)
- Returning domain entities from CachedRepository (use Response DTOs)
- Writing to database in integration tests (use API endpoints)
- Missing DataAnnotations validation on commands/queries
- Using `IConfiguration` directly (use `IOptions<T>`)
- Making endpoints or DbContext public (must be internal)
