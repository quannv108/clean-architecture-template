---
type: Term
title: "Decorator"
description: "A wrapper around a handler that adds cross-cutting behaviour without the handler knowing."
tags: [application, cross-cutting, di, scrutor]
status: stable
---

# Decorator

A class implementing the same interface as the thing it wraps, taking the inner instance as a dependency.
Registered with Scrutor's `Decorate<,>`.

Four wrap every handler here - logging, concurrency translation, validation and tracing - so none of that
appears in handler code. **Registration order is load-bearing**: later registration means further out.

See [Decorator Pipeline](../architecture/cross-cutting/decorator-pipeline.md).
