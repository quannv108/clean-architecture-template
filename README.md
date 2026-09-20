# Clean Architecture Template

A .NET 10 Clean Architecture template implementing Domain-Driven Design with CQRS and Vertical Slice Architecture.

> **📘 For AI assistants (Claude, Cursor, Copilot, Gemini, Qwen, Antigravity, …)**: start at
> **[`AGENTS.md`](AGENTS.md)**. All project knowledge lives in **[`.okf/`](.okf/index.md)**, an
> [Open Knowledge Format](https://github.com/GoogleCloudPlatform/open-knowledge-format) bundle
> with one concept per file — architecture, components, domains, decisions, patterns, conventions,
> constraints, workflows and glossary.

## What's Included

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
dotnet build CleanArchitecture.slnx          # Build
dotnet test CleanArchitecture.slnx           # Run all tests
dotnet run --project src/AppHost             # Run the full stack with .NET Aspire
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

Generate coverage locally with `./scripts/ci-local.sh` (Linux/macOS) or `scripts\ci-local.bat` (Windows). The manual
command set is documented in [.okf/workflows/engineering/generate-coverage.md](.okf/workflows/engineering/generate-coverage.md).

## Documentation

All documentation lives in **[`.okf/`](.okf/index.md)** as an Open Knowledge Format bundle — one concept per
file, so both people and agents can read exactly what they need. Architecture is described with the
[C4 model](https://c4model.com).

| Directory | Holds |
|---|---|
| [`.okf/architecture/context.md`](.okf/architecture/context.md) | **C4 L1** — the system, its users, its external dependencies |
| [`.okf/architecture/containers/`](.okf/architecture/containers/index.md) | **C4 L2** — the one deployed app, the data stores, the dev-time processes |
| [`.okf/architecture/components/`](.okf/architecture/components/index.md) | **C4 L3** — the layer projects and the inward dependency rule |
| [`.okf/architecture/cross-cutting/`](.okf/architecture/cross-cutting/index.md) | CQRS, decorators, outbox dispatch, caching, persistence, observability |
| [`.okf/architecture/delivery/`](.okf/architecture/delivery/index.md) | Local stack, test architecture, CI |
| [`.okf/engineering/`](.okf/engineering/index.md) | Code-level detail: types, patterns, dependencies, and the rules the code must keep |
| [`.okf/engineering/patterns/`](.okf/engineering/patterns/index.md) | Reusable implementation shapes with copyable code |
| [`.okf/engineering/technologies/`](.okf/engineering/technologies/index.md) | External frameworks, libraries and services |
| [`.okf/engineering/conventions/`](.okf/engineering/conventions/index.md) | Naming, placement and style rules |
| [`.okf/engineering/constraints.md`](.okf/engineering/constraints.md) | Invariants enforced by the architecture tests |
| [`.okf/domains/`](.okf/domains/index.md) | Business slices, entities and value objects — one folder per domain |
| [`.okf/adr/`](.okf/adr/index.md) | Architecture Decision Records: why it is this way |
| [`.okf/workflows/`](.okf/workflows/index.md) | Step-by-step procedures and commands — `engineering/` (build, run, test) and `process/` (branching, PRs) |
| [`.okf/glossary/`](.okf/glossary/index.md) | One file per term, tagged by subject |
| [`.okf/backlog/`](.okf/backlog/index.md) | Known gaps and planned work |

Building a product from this template? Start with
[`.okf/workflows/process/extending-the-template.md`](.okf/workflows/process/extending-the-template.md) — it explains how to
grow the knowledge base alongside your code.
