---
type: Container
title: "Customer Mobile"
description: "Planned - the mobile client for customers, installed on their devices."
resource: apps/customer-mobile
tags: [container, c4, mobile, planned]
status: draft
---

# Customer Mobile

**Placeholder.** `apps/customer-mobile/` exists but is empty. The team that builds it picks the tech stack;
record that choice here (and an ADR if it is non-obvious) when the first code lands.

## What is known

| Aspect | Value |
|---|---|
| Source | `apps/customer-mobile/` |
| Users | Customers - see [System Context](../../context.md) |
| Talks to | [Web.Api Container](../api/web-api.md) over HTTPS at `/api/v1/...` |
| Tech stack | To be decided |
| Naming | Follows [Naming and Placement](../../../engineering/conventions/naming.md) - `apps/<audience>-<platform>/` |

## To fill in when it is built

* Tech stack and how to build, run and test it
* How it authenticates against the API
* How it is released (app stores) - see [Infrastructure](../../delivery/infrastructure.md)
