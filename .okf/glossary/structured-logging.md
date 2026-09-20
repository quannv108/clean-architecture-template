---
type: Term
title: "Structured Logging"
description: "Emitting each log event as an object - a message template plus named properties - instead of a rendered string, so log tools can filter and aggregate by property."
tags: [observability, logging]
status: stable
---

# Structured Logging

A log event is emitted as data - a message template plus named properties (`Command`, `MessageId`,
`ElapsedMilliseconds`) - rather than as one pre-formatted string. The properties survive to the sink as JSON,
so a log or trace tool (Seq, Datadog, ...) can filter, group and alert on `Command = 'CreateUserCommand'`
instead of grepping text. An interpolated string throws that structure away at the call site.

Here, the `[LoggerMessage]` template names the properties, [Serilog](../engineering/technologies/serilog.md)
keeps them and ships them as JSON, and
[`RequestContextLoggingMiddleware`](../../src/Web.Api/Middleware/RequestContextLoggingMiddleware.cs) adds the
per-request correlation properties. The rule and a real instance:
[Observability](../architecture/cross-cutting/observability.md).
