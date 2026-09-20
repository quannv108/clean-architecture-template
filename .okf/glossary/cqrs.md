---
type: Term
title: "CQRS"
description: "Command Query Responsibility Segregation - separate types and paths for writes and reads."
tags: [application, cqrs, architecture]
status: stable
---

# CQRS

Writes are [commands](command.md) with handlers that mutate entities; reads are [queries](query.md) with
handlers that project to DTOs. They do not share a model: writes use the entity model, reads use response
shapes.

Here that separation is also a data-path separation - writes through
[`IApplicationDbContext`](../../src/Application/Abstractions/Data/IApplicationDbContext.cs), reads through a
[cached repository](../../src/Application) or an untracked projection. See
[CQRS](../architecture/cross-cutting/cqrs.md).

Note this is CQRS without event sourcing and without separate databases - the lightweight form.
