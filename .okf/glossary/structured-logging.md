---
type: Term
title: "Structured Logging"
description: "Logging named properties alongside the message so logs can be queried rather than grepped."
tags: [observability, logging]
status: stable
---

# Structured Logging

```csharp
logger.LogInformation("Email {Id} sent", id);      // queryable: filter by Id
logger.LogInformation($"Email {id} sent");         // a string; nothing to filter on
```

Always the first form. [`LoggingDecorator`](../../src/Application/Abstractions/Behaviors/LoggingDecorator.cs) already logs handler
start, success and failure - do not log the same thing again from inside a handler.

**Never log secrets or personal data.** See [Observability](../architecture/cross-cutting/observability.md).
