---
type: Technology
title: "Testcontainers"
description: "Real PostgreSQL in a container for integration tests."
resource: https://dotnet.testcontainers.org
tags: [technology, testing, containers, integration-tests]
status: stable
---

# Testcontainers

Used by [`ApiTestFactory`](../../../tests/Api.IntegrationTests/Infrastructure/ApiTestFactory.cs) to start a real PostgreSQL instance per test
run.

Real PostgreSQL rather than an in-memory provider because the behaviour this codebase depends on -
`xmin` concurrency, global query filters, string-stored enums, advisory locks - is exactly the behaviour an
in-memory provider does not reproduce. A test suite that passes against in-memory and fails against
PostgreSQL is worse than no suite.

**Testcontainers talks to a Docker-compatible socket directly and ignores Aspire's runtime setting.** Under
Podman it needs `DOCKER_HOST` and `TESTCONTAINERS_RYUK_DISABLED` - see
[Run Integration Tests](../../workflows/engineering/run-integration-tests.md).

Container startup dominates the first test, so the factory is shared across a collection.
