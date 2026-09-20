---
type: Term
title: "Health Check"
description: "An endpoint reporting whether the application and its dependencies are usable."
tags: [observability, operations, health, aspire]
status: stable
---

# Health Check

`/health` aggregates the registered checks - PostgreSQL always, Redis when configured - and `/alive` is the
liveness probe. Both come from [`ServiceDefaults`](../architecture/components/service-defaults.md).

The distinction matters to an orchestrator: liveness failing means restart me; readiness failing means stop
sending me traffic. Restarting a process because its database is briefly unreachable makes an outage worse.
