---
type: Term
title: "Route Group"
description: "A prefix and shared configuration applied to a set of endpoints."
tags: [http, web-api, routing, versioning]
status: stable
---

# Route Group

`app.MapGroup("api/v1")` in [`EndpointExtensions`](../../src/Web.Api/Extensions/EndpointExtensions.cs) wraps every
endpoint.

**The path written in an endpoint file is relative to the group.** `MapPost("/users", ...)` serves
`/api/v1/users`, and calling the bare path returns 404 - the most common mistake made against this
codebase, from clients and tests alike.
