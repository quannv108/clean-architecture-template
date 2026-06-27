# CLAUDE.md

A .NET 10 Clean Architecture template implementing DDD with CQRS and Vertical Slice Architecture.

This file is a concise entry point. Detailed guidance lives in `docs/` — read the relevant document before working in an area.

## Critical Rules

- **`main` is the default branch.** Always create PRs that target `main`; feature branches branch off and merge back to `main`.
- **Always run architecture tests before completing any work:** `dotnet test tests/ArchitectureTests/`
- **No MediatR.** Handlers (`ICommandHandler<T>` / `IQueryHandler<T,R>`) are registered via Scrutor and injected directly into endpoints — there is no `IMediator.Send()`.
- **Respect layer dependencies** (enforced by ArchitectureTests): SharedKernel ← Domain ← Application ← Infrastructure ← Web.Api.
- **Use `IOptions<T>`, never `IConfiguration` directly.** Keep endpoints and DbContext `internal`.

## Quick Commands

```bash
dotnet build CleanArchitecture.slnx          # Build
dotnet test CleanArchitecture.slnx           # Run all tests
dotnet test tests/ArchitectureTests/         # Architecture tests (run before completing work)
dotnet run --project src/AppHost             # Run full stack with Aspire
```

See [docs/DevelopmentGuideline.md](docs/DevelopmentGuideline.md) for the full command set, formatting, testing, and migrations.

## Documentation

Start at [docs/index.md](docs/index.md) for the full index. Key documents:

- [Architecture](docs/Architecture.md) — layers, CQRS/decorator pipeline, data access, domain events, code & naming conventions, error codes
- [Development Guideline](docs/DevelopmentGuideline.md) — branching, build/test/format commands, testing requirements, EF migrations, common pitfalls, temporary suppressions
- [Vertical Slice Structure](docs/VerticalSliceStructure.md) — feature layout, SharedKernel placement, naming table
- [Feature Templates](docs/FeatureTemplates.md) — Simple/Medium/Complex feature templates

When updating documentation, avoid content duplication across CLAUDE.md and `docs/` files; cross-reference instead of repeating, and fix existing references when content moves.
