---
type: Term
title: "Structured Logging"
description: "Logging named properties alongside the message so logs can be queried rather than grepped."
tags: [observability, logging]
status: stable
---

# Structured Logging

```csharp
[LoggerMessage(Level = LogLevel.Information, Message = "Email {Id} sent")]
private partial void LogEmailSent(Guid id);        // queryable: filter by Id; typed, no boxing

logger.LogInformation($"Email {id} sent");         // a string; nothing to filter on - and CA1848 rejects it
```

Always the first form - a `[LoggerMessage]` partial method in the owning class ([Observability](../architecture/cross-cutting/observability.md)). [`LoggingDecorator`](../../src/Application/Abstractions/Behaviors/LoggingDecorator.cs) already logs handler
start, success and failure - do not log the same thing again from inside a handler.

**Never log secrets or personal data.** See [Observability](../architecture/cross-cutting/observability.md).
