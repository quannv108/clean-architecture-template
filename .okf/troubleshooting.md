---
type: Reference
title: "Troubleshooting by Symptom"
description: "The most common failure symptoms in this template, each mapped to its likely cause and the concept that explains it."
tags: [troubleshooting, symptoms, debugging]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# Troubleshooting by Symptom

Indexed by symptom, because that is how most questions actually arrive. Each row names the likely cause and
the concept that explains it; fix the cause there, not the symptom here.

| Symptom | Likely cause |
|---|---|
| A new endpoint returns **404** | The path is relative to the `api/v1` group — [Route Conventions](engineering/conventions/route-conventions.md). Or the class does not implement `IEndpoint`, so it silently never registered — [Minimal API Endpoint](engineering/patterns/minimal-api-endpoint.md) |
| The command succeeded but **the side effect never happened** | The outbox message is pending or failed — [Domain Event Dispatch](architecture/cross-cutting/domain-event-dispatch.md). Or a handler used `ExecuteUpdate` — [Data Access](architecture/cross-cutting/data-access.md) |
| An integration test **passes locally, fails in CI** (or is flaky) | Missing `WaitForOutboxMessagesAsync()`, or the test writes to the database directly — [Test Architecture](architecture/delivery/test-architecture.md) |
| The API returns **HTTP 412** | An optimistic concurrency conflict; the client should reload and retry — [Optimistic Concurrency](engineering/patterns/optimistic-concurrency.md) |
| **The update worked but the API still shows the old value** | A cached read was not invalidated — [Cached Read](engineering/patterns/cached-read.md) |
| **Configuration is not picked up** | `IConfiguration` was injected instead of `IOptions<T>` — [Options Pattern](engineering/patterns/options-pattern.md) |
| A **deleted row is still visible** (or a unique key now clashes) | Soft delete — [Soft Delete](engineering/patterns/soft-delete.md) |
| **`Encryption key version X not found`** | A legacy key was removed before all rows were re-encrypted — [Field Encryption and Key Rotation](engineering/patterns/encryption.md) |
| **`dotnet test tests/ArchitectureTests/` fails** | A structural rule — find it in [Constraints](engineering/constraints.md); it is a design error, not a test to loosen |
| A **migration generates a wrong diff** | A migration file was deleted by hand — [Add an EF Core Migration](workflows/engineering/add-ef-migration.md) |
| **`python3 .okf/tools/okf.py` fails** | The knowledge base is inconsistent with itself — [Maintain the Knowledge Base](workflows/process/maintain-the-knowledge-base.md) |
