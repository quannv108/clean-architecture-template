---
type: Mechanism
title: Decorator Pipeline
description: The four Scrutor decorators wrapped around every command and query handler, outermost to innermost.
resource: src/Application/Abstractions/Behaviors
tags: [decorators, cross-cutting, scrutor, pipeline]
status: stable
---

# Decorator Pipeline

Configured in `src/Application/DependencyInjection.cs` with Scrutor's `Decorate<,>`. Registration order
determines nesting; the list below is **outermost first**.

```
LoggingDecorator
  -> ConcurrencyExceptionDecorator
    -> ValidationDecorator
      -> OpenTelemetryInstrumentDecorator
        -> the handler
```

| Decorator | Responsibility |
|---|---|
| [`LoggingDecorator`](../../../src/Application/Abstractions/Behaviors/LoggingDecorator.cs) | Logs handler start, completion and failure, with the operation name |
| [`ConcurrencyExceptionDecorator`](../../../src/Application/Abstractions/Behaviors/ConcurrencyExceptionDecorator.cs) | Catches `DbUpdateConcurrencyException` and returns `ConcurrencyErrors.UpdateConflict()` -> HTTP 412 |
| [`ValidationDecorator`](../../../src/Application/Abstractions/Behaviors/ValidationDecorator.cs) | Runs `DataAnnotations.Validator` on the command before the handler executes |
| [`OpenTelemetryInstrumentDecorator`](../../../src/Application/Abstractions/Behaviors/OpenTelemetryInstrumentDecorator.cs) | Opens an `Activity` span carrying operation metadata |

## Why the order matters

* Logging is outermost so it sees everything, including a validation rejection.
* Concurrency translation sits above validation so that a `DbUpdateConcurrencyException` thrown deep in
  `SaveChangesAsync` is converted to a `Result` before logging records the outcome.
* Validation sits above the handler so an invalid command never reaches business logic.
* Instrumentation sits innermost so the span measures the handler, not the cross-cutting work around it.

## What this means when you write a handler

Do not log entry and exit, do not validate the command by hand, do not catch
`DbUpdateConcurrencyException`, and do not start an `Activity`. All four are already done. A handler that
does them again produces duplicate telemetry and duplicated log lines.

## Validation is shape only

A failed attribute returns `ErrorType.Validation` (HTTP 400) and the handler never runs; the attributes must
sit on properties, which is why [Record Syntax](../../engineering/conventions/record-syntax.md) is what it is.
Custom shape rules live in
`Application/Abstractions/Validation/`; `[RegularId]` rejects `Guid.Empty`, which model binding produces from
a missing id and which would otherwise surface as a spurious "not found". Rules that need the database are
business rules for the handler or the domain -
[ADR 0013](../../adr/0013-dataannotations-validation.md).

## Adding a decorator

Implement the handler interface, take the inner handler as a constructor dependency, and register it in
`Application/DependencyInjection.cs` in the position you want - remembering that later registration means
further out. Keep it in `Application/Abstractions/Behaviors/`.
