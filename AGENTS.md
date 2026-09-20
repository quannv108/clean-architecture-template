# AGENTS.md

A .NET 10 Clean Architecture template: DDD with CQRS and Vertical Slice Architecture.

**All project knowledge lives in [`.okf/`](.okf/index.md)** — an Open Knowledge Format bundle, one concept
per file. Start at [`.okf/index.md`](.okf/index.md) and read only what you need. Do not add knowledge to
this file; add a concept file to `.okf/` and, at most, a link here.

## Five rules that override anything else you infer

1. **`main` is the default branch.** Feature branches branch off `main` and pull requests target `main`.
2. **Run `dotnet test tests/ArchitectureTests/` before completing any work.** A failure there is a design
   error, not a test to loosen — [`.okf/engineering/constraints.md`](.okf/engineering/constraints.md) explains each rule.
3. **No MediatR.** Handlers (`ICommandHandler<T>` / `IQueryHandler<T,R>`) are registered via Scrutor and
   injected directly into endpoints. There is no `IMediator.Send()`.
4. **Dependencies point inward:** SharedKernel ← Domain ← Application ← Infrastructure ← Web.Api.
5. **Use `IOptions<T>`, never `IConfiguration`.** Keep endpoints and DbContext `internal`.

## Two things that cause most wasted time

- **Every endpoint is mounted under `api/v1`.** The path in an endpoint file is relative, so `/users` is
  really `/api/v1/users`. Calling the bare path returns 404.
- **Domain events are dispatched asynchronously via the Outbox**, after the response is sent. Integration
  tests must call `WaitForOutboxMessagesAsync()` before asserting a side effect.

## Commands

```bash
dotnet build CleanArchitecture.slnx          # Build
dotnet test CleanArchitecture.slnx           # All tests
dotnet test tests/ArchitectureTests/         # Run before completing work
dotnet run --project src/AppHost             # Full stack via Aspire (Podman by default)

dotnet format CleanArchitecture.slnx style --verify-no-changes --severity error   # CI style gate
```

Only fix formatting violations in files you created or modified.

`reference/` is a read-only example solution from a different product. It is **not** part of the build and
**not** authoritative — if a grep returns two implementations of something, that is why.

## Where to look

Architecture is described with the C4 model — pick the level your question is at.

| Task | Read |
|---|---|
| Understand the system | [`.okf/architecture/context.md`](.okf/architecture/context.md) (C4 L1) |
| What runs where | [`.okf/architecture/containers/`](.okf/architecture/containers/index.md) (L2) |
| What may depend on what | [`.okf/architecture/components/`](.okf/architecture/components/index.md) (L3) |
| How a mechanism works | [`.okf/architecture/cross-cutting/`](.okf/architecture/cross-cutting/index.md) |
| Code-level detail (types, patterns, deps, rules) | [`.okf/engineering/`](.okf/engineering/index.md) |
| What a type is and where it lives | The `.cs` file (L4) — [`.okf/architecture/components/`](.okf/architecture/components/index.md) links each layer's files |
| The business model | [`.okf/domains/`](.okf/domains/index.md) |
| Adding a feature | [`.okf/workflows/engineering/add-a-feature.md`](.okf/workflows/engineering/add-a-feature.md) |
| Naming or placing a new file | [`.okf/engineering/conventions/`](.okf/engineering/conventions/index.md) |
| Rules you must not break | [`.okf/engineering/constraints.md`](.okf/engineering/constraints.md) |
| Why something is the way it is | [`.okf/adr/`](.okf/adr/index.md) |
| A command you half-remember | [`.okf/workflows/`](.okf/workflows/index.md) |
| Every file at once, to grep | [`.okf/map.md`](.okf/map.md) — generated; regenerate with `python3 .okf/tools/okf.py` |
| An unfamiliar term | [`.okf/glossary/`](.okf/glossary/index.md) |
| Using this template for a new product | [`.okf/workflows/process/extending-the-template.md`](.okf/workflows/process/extending-the-template.md) |

## When you learn something

Update `.okf/` in the same commit as the code — procedure and conventions in
[`maintain-the-knowledge-base.md`](.okf/workflows/process/maintain-the-knowledge-base.md); run
`python3 .okf/tools/okf.py` before finishing. A concept file that disagrees with the code is a defect.
`.okf` is not a place which reflect 1-1 to the code, if the code is self explained, link the code file instead create new file in `.okf`