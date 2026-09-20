---
type: Term
title: "API Versioning"
description: "Serving more than one version of an API contract."
tags: [http, api, versioning]
status: stable
---

# API Versioning

Here: a path prefix. Every endpoint is mounted under `api/v1` by
[`EndpointExtensions`](../../src/Web.Api/Extensions/EndpointExtensions.cs), and version `1.0` is the default so clients
send no version parameter.

A future v2 would be a second group with its own endpoint folder. Changing a v1 response shape is a
breaking change for every existing client - and so is renaming an [`Error.Code`](../../src/SharedKernel/Error.cs),
which clients also branch on.
