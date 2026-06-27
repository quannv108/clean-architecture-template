# Clean Architecture Template

A .NET 10 Clean Architecture template implementing Domain-Driven Design with CQRS and Vertical Slice Architecture.

> **📘 For AI Assistants (Google Antigravity, Cursor, Claude, Qwen, Gemini, etc.)**: Always reference
> **[`CLAUDE.md`](CLAUDE.md)** before working on this codebase. It is the concise entry point to architecture
> patterns, naming conventions, layer dependencies, and development standards. See [`docs/index.md`](docs/index.md)
> for the full documentation index.

## What's Included

- **SharedKernel** — common DDD abstractions (`Entity`, `ValueObject`, `Result<T>`, `Error`, `IDomainEvent`, `EncryptedString`).
- **Domain** — sample entities, domain events, and value objects with pure business logic.
- **Application** — CQRS handlers, cross-cutting concerns (logging, validation), and example use cases.
- **Infrastructure** — authentication, permission authorization, EF Core + PostgreSQL, Serilog, and Outbox processing.
- **Web.Api** — minimal API endpoints using the `IEndpoint` pattern.
- **Observability** — Serilog structured logging with [Seq](https://datalust.co/seq) (http://localhost:8081 by default).
- **Tests** — architecture, unit (NetArchTest, NSubstitute, Shouldly), and API integration tests (Testcontainers).
- **CI** — GitHub Actions for build, test, and code-coverage reporting.

## Getting Started

```bash
dotnet build CleanArchitecture.slnx          # Build
dotnet test CleanArchitecture.slnx           # Run all tests
dotnet run --project src/AppHost             # Run the full stack with .NET Aspire
```

See [docs/DevelopmentGuideline.md](docs/DevelopmentGuideline.md) for the full command set, formatting, testing
requirements, and EF Core migration workflow.

## Continuous Integration & Coverage

GitHub Actions builds the solution, runs all tests (including architecture tests), and publishes coverage on every
push and pull request:

- **Coverage report** (main branch): https://quannv108.github.io/clean-architecture-template/
- **PR summary**: coverage is added automatically to pull request checks.
- **Artifacts**: coverage reports are downloadable from the Actions tab.

Generate coverage locally with `./scripts/ci-local.sh` (Linux/macOS) or `scripts\ci-local.bat` (Windows). The manual
command set is documented in [docs/DevelopmentGuideline.md](docs/DevelopmentGuideline.md#build-and-test-commands).

## Documentation

Start at **[docs/index.md](docs/index.md)** for the full index. Highlights:

- [Architecture](docs/Architecture.md) — layers, CQRS/decorator pipeline, data access, domain events, conventions.
- [Vertical Slice Structure](docs/VerticalSliceStructure.md) — feature organization and placement rules.
- [Feature Templates](docs/FeatureTemplates.md) — step-by-step templates for adding Simple/Medium/Complex features.
