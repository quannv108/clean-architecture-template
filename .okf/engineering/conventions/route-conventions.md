---
type: Convention
title: "Route Conventions"
description: "Paths are relative to the api/v1 group; resources are plural nouns and operations are HTTP verbs."
tags: [routing, http, api, conventions]
status: stable
---

# Route Conventions

## Paths

Every path is relative to the `api/v1` group - `MapPost("/features", ...)` is served at `/api/v1/features`;
[API Surface](../../architecture/cross-cutting/api-surface.md) explains the group.

| Rule | Example |
|---|---|
| Plural resource nouns, lowercase | `/orders`, `/users` |
| Identifiers as typed route parameters | `/orders/{id:guid}` |
| The verb is the HTTP method, not the path | `POST /orders`, not `/orders/create` |
| Sub-resources nest | `/orders/{id:guid}/items` |
| Non-CRUD actions are a sub-path | `POST /orders/{id:guid}/confirm` |
| Filters and paging are query parameters | `/orders?status=pending&take=50` |

Route constraints (`{id:guid}`) are not decoration - they make a malformed id a 404 at routing time instead
of a binding failure inside the handler.

## Tags

Every endpoint sets `.WithTags(Tags.<Feature>)`, with the constant in [`Tags`](../../../src/Web.Api/Endpoints/Tags.cs). Tags
group the generated OpenAPI document, so a literal or inconsistent string splits one feature across two
sections in the docs and in generated clients.

Related: [API Surface](../../architecture/cross-cutting/api-surface.md),
[Minimal API Endpoint](../patterns/minimal-api-endpoint.md).
