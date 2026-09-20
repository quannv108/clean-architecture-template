---
type: Technology
title: "Scrutor"
description: "Assembly-scanning DI registration and decoration - how handlers are registered and the decorator pipeline is built."
resource: https://github.com/khellang/Scrutor
tags: [technology, di, scrutor, cqrs]
status: stable
---

# Scrutor

Two things, both in `Application/DependencyInjection.cs`:

* **Scanning** - registers every
  [`ICommandHandler`](../../../src/Application/Abstractions/Messaging/ICommandHandler.cs) /
  [`IQueryHandler`](../../../src/Application/Abstractions/Messaging/IQueryHandler.cs) /
  [`IDomainEventHandler`](../../../src/Application/Abstractions/Messaging/IQueryHandler.cs) implementation against its closed
  interface, so a new handler needs no registration line.
* **`Decorate<,>`** - wraps each handler in the
  [decorator pipeline](../../architecture/cross-cutting/decorator-pipeline.md). **Registration order is load-bearing**: later
  registration means further out.

Scrutor is what makes [no mediator](../../adr/0002-no-mediatr.md) practical - you get scanning and
cross-cutting behaviour without routing every call through a `Send()`.
