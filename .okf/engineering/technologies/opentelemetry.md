---
type: Technology
title: "OpenTelemetry"
description: "Traces and metrics, wired by ServiceDefaults."
resource: https://opentelemetry.io
tags: [technology, tracing, metrics, observability]
status: stable
---

# OpenTelemetry

Wired by [`ServiceDefaults`](../../architecture/components/service-defaults.md) with the standard ASP.NET Core, HttpClient and
runtime instrumentation.

[`OpenTelemetryInstrumentDecorator`](../../../src/Application/Abstractions/Behaviors/OpenTelemetryInstrumentDecorator.cs) opens a span per
handler, and [`ActivityEnricher`](../../../src/Web.Api/ActivityEnricher.cs) adds request attributes at the HTTP
layer - so a trace shows handler execution nested inside the request.

Do not start your own `Activity` for a handler; add tags to `Activity.Current` if you need more attributes.
