# Development Guideline

Day-to-day workflow, commands, and conventions for working in this codebase. For architecture and design patterns, see [Architecture.md](Architecture.md).

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

## Container runtime (Docker vs Podman)

Aspire defaults to **Podman** for local containers (`db`/Postgres, `pgweb`, `seq`) — set via `DOTNET_ASPIRE_CONTAINER_RUNTIME` in `src/AppHost/AppHost.cs`, before the builder is created. No per-developer setup is required.

To use Docker instead, set the env var before launching — it overrides the in-code default:

```powershell
# CLI
$env:DOTNET_ASPIRE_CONTAINER_RUNTIME='docker'; dotnet run --project src/AppHost
```

```json
// VS Code: add to the "Run Aspire AppHost" launch config's "env" block
"env": {
    "DOTNET_ASPIRE_CONTAINER_RUNTIME": "docker"
}
```

**Prerequisite (Podman):** the Podman machine must be running — check with `podman machine list`.

**Persistence caveat:** `ContainerLifetime.Persistent` containers and named volumes created under Docker do not migrate to Podman. Switching runtimes gives you fresh containers/volumes, so dev Postgres data starts empty (re-run migrations/seed as needed). This is expected, not a bug.

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

## Development Workflow

* Follow a TDD workflow for class enhancements: write/adjust tests first, then implement, and confirm all tests pass before finishing.

## Testing Requirements

- **Unit tests**: 70%+ coverage for Application layer, use NSubstitute (not Moq), use `BuildMock()` from MockQueryable.NSubstitute for DbSet mocking
- **Integration tests**: MUST use API endpoints only — NEVER write directly to database. Use `ApiTestFactory` (WebApplicationFactory + Testcontainers PostgreSQL). Use `ApiClient` helper for HTTP calls with auth. Call `WaitForOutboxMessagesAsync()` when testing domain event side effects.
- **Architecture tests**: Enforce layer dependencies via NetArchTest.Rules
- **Assertions**: Use Shouldly (not FluentAssertions)

## Testing & Verification

- Run architecture/layer-placement tests and verify they pass after any structural or layer placement changes.

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

## EF Core Data Access: Load → Calculate → Save

**Rule of thumb: 1 load (all data upfront) → N in-memory mutations → 1 save.** EF's `DbContext` is a Unit of Work — a single `SaveChangesAsync()` wraps all pending changes (including Outbox messages) in one atomic transaction.

> Pattern priority order, transaction handling, and `ExecuteUpdate`/`ExecuteDelete` caveats: [Concurrency.md](Concurrency.md).

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
- Using `ExecuteUpdate`/`ExecuteDelete` in command handlers — bypasses domain events, change tracking, and the Outbox; side effects silently won't fire
- Adding pessimistic locks before trying optimistic concurrency — `xmin` handles concurrent entity writes automatically
- Not implementing `IEndpoint` on Web.Api endpoints, or putting handler logic inline in the `MapEndpoint` lambda instead of a `private static HandleAsync` method
- Missing `.Accepts<T>("application/json")` on POST/PUT endpoints, or missing `.ProducesProblem()`, `.WithTags()`, or `.AddOpenApiOperationTransformer` declarations
- Calling an endpoint by the bare path from its file (e.g. `/emails/test-send`) — every endpoint is mounted under the `api/v1` group, so the real URL is `/api/v1/emails/test-send` (404 otherwise). See [Architecture.md → Route prefix (`api/v1`)](Architecture.md#cqrs-scrutor-decorators-not-mediatr)
