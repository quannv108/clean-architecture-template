---
type: Workflow
title: "Branching and Pull Requests"
description: "How branches and pull requests work in this repository."
tags: [workflow, git, branching, pull-requests, process]
status: stable
---

# Branching and Pull Requests

* **`main` is the default branch.** Every pull request targets `main`.
* Feature branches branch off `main` and merge back to `main`.
* **Run `dotnet test tests/ArchitectureTests/` before opening the pull request** -
  [Constraints](../../engineering/constraints.md).

## Before you open it

The pre-commit hook (`.githooks/pre-commit`, installed by any `dotnet build`) formats staged `.cs` files
([Format Code](../engineering/format-code.md)) and blocks a commit that leaves `.okf/` inconsistent
([Maintain the Knowledge Base](maintain-the-knowledge-base.md)).

```bash
dotnet build CleanArchitecture.slnx
dotnet test CleanArchitecture.slnx
dotnet format CleanArchitecture.slnx style --verify-no-changes --severity error
```

- [ ] [Add a Feature](../engineering/add-a-feature.md) walked, if the change adds a
      feature
- [ ] `.okf/` updated in the **same commit** - a new concept file, or a correction to an existing one
- [ ] A line added to [Knowledge Base Update Log](../../log.md) if the knowledge base changed
- [ ] An [ADR](../../adr/index.md) written if you made a decision a reasonable engineer would question

## CI

[GitHub Actions](../../architecture/delivery/ci-pipeline.md) builds, tests and publishes coverage on every push and pull
request, plus a security scan. A failing architecture test is a design error - fix the code, not the test.
