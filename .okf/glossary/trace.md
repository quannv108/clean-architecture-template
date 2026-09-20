---
type: Term
title: "Trace"
description: "The tree of spans describing one end-to-end operation."
tags: [observability, tracing, opentelemetry]
status: stable
---

# Trace

Here a trace covers the HTTP request and the handler span inside it. It does **not** extend into
[domain event handlers](../../src/Application/Abstractions/Messaging/IQueryHandler.cs), because those run later in a different
process context - which is a property of [eventual consistency](eventual-consistency.md), not a gap in
instrumentation.

To connect the two, carry a correlation identifier in the event.
