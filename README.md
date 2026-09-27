# Clean Architecture Template

A .NET 10 Clean Architecture template implementing Domain-Driven Design with CQRS and Vertical Slice Architecture.

## Repository Layout

A monorepo. Each app owns its folder and picks its own tech stack.

| Path | Holds | Status |
|---|---|---|
| [`api/`](api/) | .NET 10 backend (this template) | Ready |
| [`apps/admin-web/`](apps/admin-web/) | Admin back-office web app | Empty - stack to be decided |
| [`apps/customer-web/`](apps/customer-web/) | Customer web app | Empty - stack to be decided |
| [`apps/customer-mobile/`](apps/customer-mobile/) | Customer mobile app | Empty - stack to be decided |
| [`infra/`](infra/) | Infrastructure-as-code | Empty - tooling to be decided |
| [`.okf/`](.okf/index.md) | Project knowledge base | Ready |

## What's Included in `api/`

- **SharedKernel** — common DDD abstractions (`Entity`, `ValueObject`, `Result<T>`, `Error`, `IDomainEvent`, `EncryptedString`).
- **Domain** — sample entities, domain events, and value objects with pure business logic.
- **Application** — CQRS handlers, cross-cutting concerns (logging, validation), and example use cases.
- **Infrastructure** — authentication, permission authorization, EF Core + PostgreSQL, Serilog, and Outbox processing.
- **Web.Api** — minimal API endpoints using the `IEndpoint` pattern.
- **Observability** — Serilog structured logging with [Seq](https://datalust.co/seq) (http://localhost:8081 by default).
- **Tests** — architecture, unit (NetArchTest, NSubstitute, Shouldly), and API integration tests (Testcontainers).
- **CI** — GitHub Actions for build, test, and code-coverage reporting.

## Prerequisites

- **[.NET 10 SDK](https://dotnet.microsoft.com/download)** — the template targets .NET 10.
- **Podman** — the default container runtime .NET Aspire uses to run Postgres, pgweb, and Seq. The Podman machine must
  be running. Docker can be used instead by overriding the runtime; see
  [.okf/workflows/engineering/run-the-stack.md](.okf/workflows/engineering/run-the-stack.md) for runtime selection details.

## Getting Started

```bash
dotnet build api/CleanArchitecture.slnx          # Build
dotnet test api/CleanArchitecture.slnx           # Run all tests
dotnet run --project api/src/AppHost             # Run the full stack with .NET Aspire
```

See [.okf/workflows/index.md](.okf/workflows/index.md) for the full command set, formatting, testing
requirements, and the EF Core migration workflow.

The first build also points git at the committed hooks (`git config core.hooksPath .githooks`), so every
clone gets the pre-commit hook without a setup step: it runs `dotnet format` on staged `.cs` files and checks
`.okf/` when it is staged.

## Continuous Integration & Coverage

GitHub Actions builds the solution, runs all tests (including architecture tests), and publishes coverage on every
push and pull request:

- **Coverage report** (main branch): https://quannv108.github.io/clean-architecture-template/
- **PR summary**: coverage is added automatically to pull request checks.
- **Artifacts**: coverage reports are downloadable from the Actions tab.

Generate coverage locally with `./api/scripts/ci-local.sh` (Linux/macOS) or `api\scripts\ci-local.bat` (Windows). The manual
command set is documented in [.okf/workflows/engineering/generate-coverage.md](.okf/workflows/engineering/generate-coverage.md).

## Documentation

All project knowledge lives in **[`.okf/`](.okf/index.md)** (an
[Open Knowledge Format](https://github.com/GoogleCloudPlatform/open-knowledge-format) bundle). Start at
[`.okf/index.md`](.okf/index.md). AI assistants start at [`AGENTS.md`](AGENTS.md).
