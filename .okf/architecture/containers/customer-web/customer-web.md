---
type: Container
title: "Customer Web"
description: "Planned - the customer-facing web app that end users reach the system through."
resource: apps/customer-web
tags: [container, c4, frontend, planned]
status: draft
---

# Customer Web

**Placeholder.** `apps/customer-web/` exists but is empty. The team that builds it picks the tech stack;
record that choice here (and an ADR if it is non-obvious) when the first code lands.

## What is known

| Aspect | Value |
|---|---|
| Source | `apps/customer-web/` |
| Users | Customers - see [System Context](../../context.md) |
| Talks to | [Web.Api Container](../api/web-api.md) over HTTPS at `/api/v1/...` |
| Tech stack | To be decided |
| Naming | Follows [Naming and Placement](../../../engineering/conventions/naming.md) - `apps/<audience>-<platform>/` |

## To fill in when it is built

* Tech stack and how to build, run and test it
* How it authenticates against the API
* How it is deployed - see [Infrastructure](../../delivery/infrastructure.md)
