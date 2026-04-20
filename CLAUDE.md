# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Branching

- **`main` is the default branch.** Always create PRs that target `main`.
- Feature branches branch off `main` and merge back to `main`.
- Always run architecture tests before completing any work: `dotnet test tests/ArchitectureTests/`

## Build and Test Commands

```bash
# Build solution
dotnet build CleanArchitecture.slnx

# Run all tests
dotnet test CleanArchitecture.slnx

# Run specific test projects
dotnet test tests/ArchitectureTests/       # Architecture tests (always run before completing work)
dotnet test tests/Application.UnitTests/  # Unit tests
dotnet test tests/Api.IntegrationTests/   # Integration tests

# Run with Aspire (full stack with containers)
dotnet run --project src/AppHost

# Local CI pipeline
./scripts/ci-local.sh        # Linux/macOS
scripts\ci-local.bat         # Windows

# Generate code coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:"Html"
```

## Code Formatting

```bash
# Check for style violations (read-only)
dotnet format CleanArchitecture.slnx style --verify-no-changes --severity error

# Auto-fix style violations
dotnet format CleanArchitecture.slnx style

# Filter to only files you changed (example)
dotnet format CleanArchitecture.slnx style --verify-no-changes --severity error 2>&1 | grep "src/Application/MyFeature"
```

> **Note:** Only fix formatting violations in files you created or modified — the solution may have pre-existing violations in unrelated files.

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

## EF Core Migrations

Migrations live in `src/Infrastructure/Database/Migrations`.

### Add a migration
```bash
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure \
  --startup-project src/Web.Api \
  --output-dir Database/Migrations \
  --context ApplicationDbContext \
  -- --environment Migration
```

**Common pitfalls:**
- **`--output-dir Database/Migrations` is REQUIRED.** Without it, EF generates into `src/Infrastructure/Migrations/` — wrong path, wrong namespace. EF will not discover them at runtime.
- **`-- --environment Migration` is REQUIRED.** Without it, the startup project boots with the default environment and may fail due to missing config/services.
- **Change `public partial` → `internal partial`** on both the migration class and the `.Designer.cs` class. Architecture tests enforce all Infrastructure types are `internal`.

### Remove a migration
```bash
dotnet ef migrations remove \
  --project src/Infrastructure \
  --startup-project src/Web.Api \
  --context ApplicationDbContext \
  -- --environment Migration
```

**NEVER manually delete migration files.** `migrations remove` also reverts `ApplicationDbContextModelSnapshot.cs`. Manual deletion leaves the snapshot out of sync, causing subsequent migrations to generate incorrect diffs.

### Verify after adding/removing
1. Build: `dotnet build CleanArchitecture.slnx`
2. Run architecture tests: `dotnet test tests/ArchitectureTests/`

## Error Codes

Domain error codes use the `"{Entity}.{ErrorName}"` pattern (e.g. `"User.NotFound"`, `"Order.InvalidStatusTransition"`). These go in the `Error.Code` field, defined as `public static` factory methods in `<Feature>Errors.cs`.

## Key Documentation

- `agents.md` - Comprehensive AI agent instructions
- `docs/FeatureTemplates.md` - Templates for Simple/Medium/Complex features
- `docs/VerticalSliceStructure.md` - Feature organization patterns
- `docs/DomainEvent.md` - Domain event implementation
- `docs/Caching.md` - HybridCache patterns

## Known Temporary Suppressions

### CA1873 — `#pragma warning disable CA1873` (temporary)

`Microsoft.Extensions.Logging.Abstractions 10.0.3` introduced a stricter CA1873 analyzer that fires on logger calls passing `DateTime`, property accesses, and other value types — even when captured in local variables. This is a known regression in the .NET 10 analyzer.

**Suppressed in these files** (both Application and Infrastructure layers):
- `Application/Abstractions/Behaviors/LoggingDecorator.cs`
- `Application/AuditLogs/DeleteOldAuditLogsCommand.cs`
- `Application/Outbox/CleanupProcessedOutboxMessagesCommand.cs`
- `Application/Outbox/IOutboxMessageProcessor.cs`
- `Application/ExampleDomainA/Events/EmailSentDomainEventHandler.cs`
- `Infrastructure/DependencyInjection.cs`
- `Infrastructure/DomainEvents/DomainEventsDispatcher.cs`
- `Infrastructure/Communication/Sms/DummySmsSender.cs`
- `Infrastructure/Communication/Sms/TwilioSmsSender.cs`

**Action**: Remove the `#pragma warning disable CA1873` lines once Microsoft fixes the analyzer in a future `Microsoft.Extensions.Logging.Abstractions` patch. Do NOT convert these to `[LoggerMessage]` source generators just to satisfy the analyzer — that would over-engineer simple log calls.

## Common Pitfalls to Avoid

- Using CachedRepository in command handlers (use DbContext directly)
- Returning domain entities from CachedRepository (use Response DTOs)
- Writing to database in integration tests (use API endpoints)
- Missing DataAnnotations validation on commands/queries
- Using `IConfiguration` directly (use `IOptions<T>`)
- Making endpoints or DbContext public (must be internal)
