---
type: Container
title: "Seq"
description: "The structured log server in the local stack, at http://localhost:8081."
tags: [container, c4, logging, observability, local-development]
status: stable
---

# Seq

Started by [AppHost](apphost.md) as part of the local stack, by default at `http://localhost:8081`.

[Serilog](../../engineering/technologies/serilog.md) writes structured properties and
[`RequestContextLoggingMiddleware`](../../../src/Web.Api/Middleware/RequestContextLoggingMiddleware.cs) adds
correlation data, so one filter reconstructs a whole request — which is the reason to use Seq locally
rather than the console.

Useful starting filters: by correlation id, by `Error.Code`, by handler name.

**Development only.** A deployed environment ships telemetry to whatever the platform provides; see
[Observability](../cross-cutting/observability.md).
