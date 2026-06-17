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

# Run a single test by name
dotnet test tests/Application.UnitTests/ --filter "FullyQualifiedName~MyTestClass.MyTestMethod"

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

- **SharedKernel** - Common DDD abstractions: `Entity`, `AuditedEntity`, `ValueObject`, `Result<T>`, `Error`, `IDomainEvent`, `EncryptedString`
- **Domain** - Pure business logic, entities, domain events, value objects
- **Application** - Use cases, CQRS handlers, CachedRepositories, decorator pipeline
- **Infrastructure** - EF Core, PostgreSQL, authentication, external services, Outbox processing
- **Web.Api** - Minimal API endpoints using `IEndpoint` pattern
- **AppHost** - .NET Aspire orchestration
- **ServiceDefaults** - Aspire service defaults (health checks, telemetry)

### CQRS: Scrutor Decorators, NOT MediatR

This codebase does **not** use MediatR. Handlers are registered via Scrutor assembly scanning and injected directly into endpoints. There is no `IMediator.Send()`.

**Handler interfaces** (in `Application/Abstractions/Messaging/`):
- `ICommandHandler<TCommand>` → returns `Task<Result>`
- `ICommandHandler<TCommand, TResponse>` → returns `Task<Result<TResponse>>`
- `IQueryHandler<TQuery, TResponse>` → returns `Task<Result<TResponse>>`
- `IDomainEventHandler<TEvent>` → returns `Task`

**Decorator pipeline** (outermost → innermost, configured in `Application/DependencyInjection.cs`):
```
LoggingDecorator → ConcurrencyExceptionDecorator → ValidationDecorator → OpenTelemetryInstrumentDecorator → Handler
```
- **LoggingDecorator**: Logs start/completion/failure of all handlers
- **ConcurrencyExceptionDecorator**: Catches `DbUpdateConcurrencyException`, returns `ConcurrencyErrors.UpdateConflict()`
- **ValidationDecorator**: Runs `DataAnnotations.Validator` on commands before handler executes
- **OpenTelemetryInstrumentDecorator**: Creates Activity spans with operation metadata

**Endpoint handler injection** — endpoints inject handlers directly:
```csharp
app.MapPost("/users", async (
    CreateUserCommand command,
    ICommandHandler<CreateUserCommand, Guid> handler,
    CancellationToken ct) =>
{
    Result<Guid> result = await handler.Handle(command, ct);
    return result.Match(Results.Ok, CustomResults.Problem);
});
```

### Data Access

- **Reads**: CachedRepository classes in `Application/<Feature>/Data/` using HybridCache, returning DTOs (never entities)
- **Writes**: Direct `IApplicationDbContext` injection in command handlers
- **Soft delete**: Global query filter on all `Entity` subclasses (`IsDeleted == false`) — applied automatically by EF Core
- **Optimistic concurrency**: PostgreSQL `xmin` column mapped to `Entity.Version` as row version
- **Enums**: Stored as strings (convention in `BaseApplicationDbContext`)
- **Encrypted fields**: Use `EncryptedString` value object with `{KeyVersion}:{EncryptedValue}` format

### Domain Events & Outbox Pattern

Domain events are dispatched **asynchronously via the Outbox pattern**, not in-memory:

1. Entity raises event: `this.Raise(new SomeDomainEvent(...))`
2. `SaveChangesAsync()` persists entity changes + creates `OutboxMessage` records
3. `OutboxMessageHostedService` polls the `OutboxMessages` table on a timer
4. For each pending message: acquires distributed lock → deserializes event → dispatches to all `IDomainEventHandler<T>` implementations → marks as Processed/Failed
5. Handlers execute under system user context (`SystemConstants.SystemUserId`)

### Vertical Slice Organization

Features organized by business capability across all layers:
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
- **Web.Api**: Positional syntax — `public record MyRecord(string Prop1, int Prop2);`
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
- IDs generated via `Guid.CreateVersion7()` (by `EntityIdGenerationInterceptor`)

### Result → HTTP Mapping
Endpoints use `Result.Match()` to convert to HTTP responses:
```csharp
result.Match(Results.Ok, CustomResults.Problem);
```
`CustomResults.Problem` maps `ErrorType` → HTTP status: Validation→400, NotFound→404, Conflict→409, Problem→412.

## Testing Requirements

- **Unit tests**: 70%+ coverage for Application layer, use NSubstitute (not Moq), use `BuildMock()` from MockQueryable.NSubstitute for DbSet mocking
- **Integration tests**: MUST use API endpoints only — NEVER write directly to database. Use `ApiTestFactory` (WebApplicationFactory + Testcontainers PostgreSQL). Use `ApiClient` helper for HTTP calls with auth. Call `WaitForOutboxMessagesAsync()` when testing domain event side effects.
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
- `docs/OutboxPattern.md` - Outbox pattern details
- `docs/Encryption.md` - Encrypted data storage
- `docs/DistributedLock.md` - Distributed locking (PostgreSQL/Redis)
- `docs/AuditLogging.md` - Audit logging (4W: Who, What, When, Where)

## Known Temporary Suppressions

### CA1873 — `#pragma warning disable CA1873` (temporary)

`Microsoft.Extensions.Logging.Abstractions 10.0.3` introduced a stricter CA1873 analyzer that fires on logger calls passing `DateTime`, property accesses, and other value types. This is a known regression in the .NET 10 analyzer.

**Suppressed in files across Application and Infrastructure layers** (search for `#pragma warning disable CA1873`).

**Action**: Remove the `#pragma warning disable CA1873` lines once Microsoft fixes the analyzer in a future patch. Do NOT convert these to `[LoggerMessage]` source generators just to satisfy the analyzer.

## Common Pitfalls to Avoid

- Using CachedRepository in command handlers (use DbContext directly)
- Returning domain entities from CachedRepository (use Response DTOs)
- Writing to database in integration tests (use API endpoints)
- Missing DataAnnotations validation on commands/queries
- Using `IConfiguration` directly (use `IOptions<T>`)
- Making endpoints or DbContext public (must be internal)
- Using MediatR patterns — this codebase injects `ICommandHandler<T>`/`IQueryHandler<T,R>` directly, no mediator
- Forgetting `-- --environment Migration` or `--output-dir Database/Migrations` in EF commands
- Forgetting to change migration visibility from `public` to `internal`
