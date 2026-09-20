---
type: Term
title: "Testcontainer"
description: "A throwaway container started by a test run to provide a real dependency."
tags: [testing, containers]
status: stable
---

# Testcontainer

Used by [`ApiTestFactory`](../../tests/Api.IntegrationTests/Infrastructure/ApiTestFactory.cs) to start real PostgreSQL per integration test
run.

Real PostgreSQL rather than an in-memory provider because the behaviour this codebase depends on -
[`xmin`](xmin.md) concurrency, [query filters](global-query-filter.md), string-stored enums, advisory locks
- is precisely what an in-memory provider does not reproduce.

Under Podman it needs `DOCKER_HOST` and `TESTCONTAINERS_RYUK_DISABLED` - see
[Run Integration Tests](../workflows/engineering/run-integration-tests.md).
