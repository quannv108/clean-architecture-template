---
type: Term
title: "OpenAPI"
description: "The machine-readable description of the HTTP API, generated from endpoint metadata."
tags: [http, api, documentation]
status: stable
---

# OpenAPI

Generated from each endpoint's builder chain. It is the contract clients and code generators consume, which
is why an undeclared response shape is a defect even when the code works.

Required per endpoint: `.Accepts<T>()` on POST/PUT, `.Produces<T>()`, `.ProducesProblem()` for each error
status, `.WithTags(Tags.<Feature>)`, and an `AddOpenApiOperationTransformer` setting `Summary` and
`Description`. See [Minimal API Endpoint](../engineering/patterns/minimal-api-endpoint.md).
