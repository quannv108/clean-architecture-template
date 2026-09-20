---
type: Term
title: "Command"
description: "A request to change state, as a record with a handler."
tags: [application, cqrs, write]
status: stable
---

# Command

A `sealed record` implementing [`ICommand`](../../src/Application/Abstractions/Messaging/ICommand.cs) or `ICommand<TResponse>`, declared in
the same file as its handler, using standard record syntax with DataAnnotations.

Named for the business action: `ConfirmOrderCommand`, not `UpdateOrderCommand`. Returns
[`Result`](../../src/SharedKernel/Result.cs).

Shape: [Command Handler](../engineering/patterns/command-handler.md).
