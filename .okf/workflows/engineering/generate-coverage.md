---
type: Workflow
title: "Generate a Coverage Report"
description: "Producing the HTML coverage report locally."
tags: [workflow, coverage, testing, reporting]
status: stable
---

# Generate a Coverage Report

```bash
cd api
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

reportgenerator \
  -reports:"coverage/**/coverage.cobertura.xml" \
  -targetdir:"coverage/report" \
  -reporttypes:"Html"
```

Or run the whole CI pipeline locally: `./api/scripts/ci-local.sh` / `api\scripts\ci-local.bat`.

Open `api/coverage/report/index.html`.

**Target: 70%+ for the Application layer.** Coverage is a smoke detector, not a goal - a handler at 100%
with no assertion on the failure branches proves nothing. Cover each `Result.Failure` path, not just the
happy one.

[CI](../../architecture/delivery/ci-pipeline.md) publishes the report for `main` and adds a summary to every pull request.
