---
type: Term
title: "Minimal API"
description: "The ASP.NET Core endpoint model used here instead of MVC controllers."
tags: [http, web-api]
status: stable
---

# Minimal API

Routes are declared directly against the app rather than through controller classes. Here each operation is
one [`IEndpoint`](../../src/Web.Api/Endpoints/IEndpoint.cs) class in its own file, discovered by scan and mounted under
`api/v1`.

See [Minimal API Endpoint](../engineering/patterns/minimal-api-endpoint.md) and
[ADR 0010: Minimal APIs with a discovered IEndpoint interface](../adr/0010-minimal-api-iendpoint.md).
