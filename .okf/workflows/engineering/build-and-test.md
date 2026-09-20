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
dotnet build CleanArchitecture.slnx

# All tests
dotnet test CleanArchitecture.slnx

# By project
dotnet test tests/ArchitectureTests/        # run before completing any work
dotnet test tests/Application.UnitTests/
dotnet test tests/Api.IntegrationTests/     # needs a container runtime

# A single test
dotnet test tests/Application.UnitTests/ --filter "FullyQualifiedName~MyTestClass.MyTestMethod"

# Full stack with Aspire
dotnet run --project src/AppHost

# The CI pipeline, locally
./scripts/ci-local.sh        # Linux/macOS
scripts\ci-local.bat         # Windows
```

## The development loop

The template follows TDD for class enhancements: adjust or write the test first, implement, then confirm the
whole suite passes.

**Before declaring anything complete:**

```bash
dotnet test tests/ArchitectureTests/
```

See [Constraints](../../engineering/constraints.md).

Related: [Format Code](format-code.md), [Run Integration Tests](run-integration-tests.md),
[Generate a Coverage Report](generate-coverage.md).
