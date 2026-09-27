---
type: Workflow
title: "Build and Test"
description: "The command set for building, testing and running the solution."
tags: [workflow, build, test, commands]
status: stable
---

# Build and Test

```bash
# Build
dotnet build api/CleanArchitecture.slnx

# All tests
dotnet test api/CleanArchitecture.slnx

# By project
dotnet test api/tests/ArchitectureTests/        # run before completing any work
dotnet test api/tests/Application.UnitTests/
dotnet test api/tests/Api.IntegrationTests/     # needs a container runtime

# A single test
dotnet test api/tests/Application.UnitTests/ --filter "FullyQualifiedName~MyTestClass.MyTestMethod"

# Full stack with Aspire
dotnet run --project api/src/AppHost

# The CI pipeline, locally
./api/scripts/ci-local.sh        # Linux/macOS
api\scripts\ci-local.bat         # Windows
```

**SDK pin caveat:** `api/global.json` pins the SDK version, but `dotnet` only reads a `global.json` from the
current directory upward — so it applies only when the command's cwd is inside `api/` (as `dotnet build
api/CleanArchitecture.slnx` from the repo root is). Aspire's own `msbuild-sdks` pin in the same file resolves
from the solution/project directory regardless of cwd, so it always applies.

## The development loop

The template follows TDD for class enhancements: adjust or write the test first, implement, then confirm the
whole suite passes.

**Before declaring anything complete:**

```bash
dotnet test api/tests/ArchitectureTests/
```

See [Constraints](../../engineering/constraints.md).

Related: [Format Code](format-code.md), [Run Integration Tests](run-integration-tests.md),
[Generate a Coverage Report](generate-coverage.md).
