---
type: Mechanism
title: Request Lifecycle
description: What happens to an HTTP request from the route table to the committed transaction and the dispatched domain event.
tags: [lifecycle, request, pipeline, end-to-end]
status: stable
---

# Request Lifecycle

The end-to-end path. Each step links to the concept that owns it.

1. **Middleware.** [`RequestContextLoggingMiddleware`](../../../src/Web.Api/Middleware/RequestContextLoggingMiddleware.cs)
   pushes correlation data into the Serilog context;
   [`GlobalExceptionHandler`](../../../src/Web.Api/Infrastructure/GlobalExceptionHandler.cs) is registered to convert anything
   unhandled into a ProblemDetails response.
2. **Routing.** The request matches an endpoint registered by
   [`EndpointExtensions.MapEndpoints`](../../../src/Web.Api/Extensions/EndpointExtensions.cs) inside the `api/v1` group. See
   [API Surface](api-surface.md).
3. **Authorization and rate limiting.** Policies declared in the endpoint's builder chain run. See
   [Security and Authorization](security-and-authorization.md).
4. **Endpoint.** `private static HandleAsync` builds the command or query from the request record and calls
   the injected handler. See [Web.Api Layer](../components/web-api.md).
5. **Decorator pipeline.** Logging, concurrency translation, validation and tracing wrap the handler. See
   [Decorator Pipeline](decorator-pipeline.md).
6. **Handler.** A query handler reads via a [cached repository](../../../src/Application); a
   command handler loads entities through [`IApplicationDbContext`](../../../src/Application/Abstractions/Data/IApplicationDbContext.cs),
   invokes behaviour on them, and calls `SaveChangesAsync` **once**. See [Data Access](data-access.md).
7. **Save.** Interceptors run -
   [`EntityIdGenerationInterceptor`](../../../src/Infrastructure/Database/Interceptors/EntityIdGenerationInterceptor.cs) assigns ids,
   [`AuditableEntityInterceptor`](../../../src/Infrastructure/Database/Interceptors/AuditableEntityInterceptor.cs) stamps audit fields - and
   raised domain events are written as [`OutboxMessage`](../../domains/outbox/outbox-message.md) rows **in the same
   transaction** as the business data. `xmin` optimistic concurrency is checked here. See
   [Optimistic Concurrency](../../engineering/patterns/optimistic-concurrency.md).
8. **Result mapping.** The endpoint calls `result.Match(Results.Ok, CustomResults.Problem)`;
   [`ErrorType`](../../../src/SharedKernel/Error.cs) becomes an HTTP status.
9. **Audit.** [`AuditLoggingMiddleware`](../../../src/Web.Api/Middleware/AuditLoggingMiddleware.cs) records Who/What/When/Where
   for endpoints marked `.WithAuditLog("...")`, fire-and-forget. See
   [Audit Logging](../../engineering/patterns/audit-logging.md).
10. **Later, out of band.**
    [`OutboxMessageHostedService`](../../../src/Infrastructure/Outbox/OutboxMessageHostedService.cs) picks up the pending
    messages and [`OutboxMessageProcessor`](../../../src/Application/Outbox) dispatches each to its
    `IDomainEventHandler<T>`. The HTTP response has already been sent. See
    [Domain Event Dispatch](domain-event-dispatch.md).

## The consequence to remember

Nothing that a domain event handler does is visible to the caller of the request that raised it. Integration
tests must call `WaitForOutboxMessagesAsync()` before asserting a side effect. See
[Run Integration Tests](../../workflows/engineering/run-integration-tests.md).
