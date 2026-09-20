---
type: Technology
title: "Seq"
description: "The structured log server used locally, at http://localhost:8081."
resource: https://datalust.co/seq
tags: [technology, logging, observability, local-development]
status: stable
---

# Seq

Started as part of the [Aspire stack](../../architecture/containers/apphost.md), by default at `http://localhost:8081`.

Because [Serilog](serilog.md) logs structured properties and
[`RequestContextLoggingMiddleware`](../../../src/Web.Api/Middleware/RequestContextLoggingMiddleware.cs) adds correlation
data, a single request can be reconstructed with one filter - which is the main reason to reach for Seq
rather than the console.

Useful starting filters: by correlation id, by `Error.Code`, by handler name.
