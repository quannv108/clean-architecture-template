---
type: Term
title: "Span"
description: "A timed unit of work within a distributed trace."
tags: [observability, tracing, opentelemetry]
status: stable
---

# Span

An `Activity` in .NET. [`OpenTelemetryInstrumentDecorator`](../../src/Application/Abstractions/Behaviors/OpenTelemetryInstrumentDecorator.cs)
opens one per handler, nested inside the HTTP span, so a [trace](trace.md) shows where the time went.

Do not start your own span for a handler - add tags to `Activity.Current` instead.
