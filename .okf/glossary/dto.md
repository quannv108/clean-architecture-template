---
type: Term
title: "DTO"
description: "A Data Transfer Object - a flat shape for moving data across a boundary, with no behaviour."
tags: [application, dto, read, api]
status: stable
---

# DTO

Here, DTOs are the `*Response` records returned by query handlers and cached repositories, and the request
records bound by endpoints.

**Reads return DTOs, never domain entities.** An entity returned from a read leaks the write model into the
API contract, and once cached it is a detached, stale, mutable copy of state the domain believes it owns.
