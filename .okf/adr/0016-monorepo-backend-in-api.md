---
type: ADR
title: "ADR 0016: Move the backend into api/ to prepare for a monorepo"
description: "Move the .NET solution and its root config into api/ so the repository can grow into a monorepo without reshuffling paths again."
tags: [adr, monorepo, repository-layout]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-27T00:00:00Z
---

# ADR 0016: Move the backend into api/ to prepare for a monorepo

**Status:** Accepted

## Context

This repository is becoming a monorepo: the .NET backend, a web app, an AI service, infrastructure code, a
mobile app, this knowledge bundle, and CI/agent tooling are all meant to live side by side. Until now the
backend owned the repository root - `src/`, `tests/`, `CleanArchitecture.slnx` and every .NET-specific root
file sat at `/`. Adding a sibling app later would force every one of those paths to move anyway, breaking
links, CI, and editor config a second time.

## Decision

Move the backend into `api/`: `src/`, `tests/`, `scripts/`, `CleanArchitecture.slnx`,
`Directory.Build.props`, `Directory.Packages.props`, `global.json`, `.editorconfig`, `.dockerignore`, and the
`docker-compose*`/`launchSettings.json` files. Everything specific to the .NET toolchain now lives under
`api/`; a future sibling (`ui/`, `apps/`, etc.) gets the same treatment without touching `api/`.

Everything that is repository-wide, or has no reason to be backend-specific, stays at the root:
`.github/`, `.githooks/`, `.claude/`, `.vscode/`, `.okf/`, `AGENTS.md`, `CLAUDE.md`, `README.md`,
`.gitignore`, `.gitattributes`, and the read-only `reference/` example solution.

CI, the pre-commit hook, editor config and this knowledge bundle were all updated in the same change so
nothing points at a path that no longer exists - see [Solution Layout](../architecture/solution-layout.md)
for the resulting root/`api/` split, and [CI Pipeline](../architecture/delivery/ci-pipeline.md) for how the
GitHub Actions jobs now scope themselves to `api/`.

`ui/`/`apps/` are not created yet - this ADR only relocates the backend.

## Consequences

**Good.** The backend is a self-contained subtree with its own solution, SDK pin and Docker context, so a
sibling app can be added later as a plain new top-level directory with no further backend churn. CI jobs
that only concern the backend (`dotnet build`/`test`, the vulnerability audit) are now visibly scoped to
`api/` via `working-directory`, rather than implicitly meaning "the whole repo".

**Costly.** Every path in this knowledge bundle, `AGENTS.md`, `.claude/rules/*.md`, `.vscode/*`, and both
GitHub Actions workflows needed an `api/` prefix in one large mechanical change. `dotnet` commands run from
the repository root now need an explicit path (`dotnet build api/CleanArchitecture.slnx`) or a `cd api`
first - `api/global.json`'s SDK pin only takes effect when the working directory is inside `api/`.

**Alternatives rejected.**

* **Keep the backend at root, add siblings elsewhere.** Rejected: a second app added later still could not
  use the root path space, so the move would happen anyway, just later and with more history to drag along.
* **A tools-only monorepo layout (`apps/api/`, `apps/web/`, ...) from day one.** Rejected as premature: no
  second app exists yet, and nesting `api/` one level deeper than necessary buys nothing until it does. This
  ADR does not preclude introducing an `apps/` umbrella later.

## When to revisit

When a second app (`ui/`, `apps/mobile`, an AI service, etc.) is actually added. At that point, decide
whether `api/` should move under a shared `apps/` parent for symmetry, or stay as a top-level sibling.
