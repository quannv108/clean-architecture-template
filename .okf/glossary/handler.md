---
type: Term
title: "Handler"
description: "The class that executes one use case - a command, a query or a domain event."
tags: [application, cqrs]
status: stable
---

# Handler

`internal sealed`, one per use case, in `Application/<Feature>/`. Registered by
[Scrutor](../engineering/technologies/scrutor.md) and injected directly - there is no mediator.

Handlers orchestrate: load, invoke domain behaviour, save, return a [`Result`](../../src/SharedKernel/Result.cs).
Business rules belong on the entity; a handler full of `if` statements about what the domain permits has
taken the domain's job.
