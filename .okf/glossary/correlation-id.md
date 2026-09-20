---
type: Term
title: "Correlation ID"
description: "An identifier shared by every log line and span belonging to one request."
tags: [observability, logging, tracing]
status: stable
---

# Correlation ID

Pushed into the Serilog context by
[`RequestContextLoggingMiddleware`](../../src/Web.Api/Middleware/RequestContextLoggingMiddleware.cs), so one filter in
[Seq](../engineering/technologies/seq.md) reconstructs a whole request.

Its value shows up during incidents, where the question is always "what else happened in that request" -
and it is why logging structured properties rather than interpolated strings matters.
