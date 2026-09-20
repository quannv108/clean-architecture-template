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
[LoggerMessage(Level = LogLevel.Information, Message = "Email {Id} sent")]
private partial void LogEmailSent(Guid id);        // queryable in Seq; typed, no boxing

logger.LogInformation($"Email {id} sent");         // a string; nothing to filter on - and CA1848 rejects it
```

[`LoggingDecorator`](../../../src/Application/Abstractions/Behaviors/LoggingDecorator.cs) already logs handler start, success and failure -
do not log the same thing again.

**Never log secrets or personal data.** See [Observability](../../architecture/cross-cutting/observability.md).
