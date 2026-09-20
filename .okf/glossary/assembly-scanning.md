---
type: Term
title: "Assembly Scanning"
description: "Registering or discovering types by reflecting over an assembly instead of listing them."
tags: [application, di, scrutor, discovery]
status: stable
---

# Assembly Scanning

Used three times here: [Scrutor](../engineering/technologies/scrutor.md) registers handlers,
[`MapEndpoints`](../../src/Web.Api/Extensions/EndpointExtensions.cs) discovers
[`IEndpoint`](../../src/Web.Api/Endpoints/IEndpoint.cs) implementations, and EF Core discovers
`IEntityTypeConfiguration<T>`.

The trade-off is silence. Adding a handler or endpoint needs no registration line - but a class that does
not match the pattern is simply not found, with **no error**. A 404 on a new endpoint is almost always a
missing `IEndpoint`.
