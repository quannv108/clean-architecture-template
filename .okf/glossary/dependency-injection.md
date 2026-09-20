---
type: Term
title: "Dependency Injection"
description: "Supplying a class's collaborators from outside rather than constructing them internally."
tags: [application, di, architecture]
status: stable
---

# Dependency Injection

The mechanism that makes the layering work: Application depends on interfaces it declares, and the
container supplies Infrastructure implementations at runtime, so the compile-time arrow still points
inward.

Registration lives in each layer's `DependencyInjection.cs` -
[the composition root](composition-root.md). Handlers are registered by
[Scrutor](../engineering/technologies/scrutor.md) scanning and wrapped in
[decorators](../architecture/cross-cutting/decorator-pipeline.md).
