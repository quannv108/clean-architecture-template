---
type: Technology
title: "Serilog"
description: "Structured logging, shipped to Seq."
resource: https://serilog.net
tags: [technology, logging, observability]
status: stable
---

# Serilog

Structured logging throughout.
[`RequestContextLoggingMiddleware`](../../../src/Web.Api/Middleware/RequestContextLoggingMiddleware.cs) pushes correlation
properties into `LogContext`, so every line in a request shares them.

**Log properties, not interpolated strings:**

```csharp
logger.LogInformation("Email {Id} sent", id);      // queryable in Seq
logger.LogInformation($"Email {id} sent");         // a string; nothing to filter on
```

[`LoggingDecorator`](../../../src/Application/Abstractions/Behaviors/LoggingDecorator.cs) already logs handler start, success and failure -
do not log the same thing again.

**Never log secrets or personal data.** See [Observability](../../architecture/cross-cutting/observability.md).
