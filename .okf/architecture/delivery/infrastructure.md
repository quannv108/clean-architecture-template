---
type: Mechanism
title: "Infrastructure"
description: "Planned - the infrastructure-as-code in infra/ that provisions the environments every container is deployed to."
resource: infra
tags: [infrastructure, deployment, iac, planned]
status: draft
---

# Infrastructure

**Placeholder.** `infra/` exists but is empty. The team that owns it picks the tooling (Terraform, Bicep,
Pulumi, Helm, ...); record that choice here (and an ADR if it is non-obvious) when the first code lands.

This page is where the C4 **deployment** view belongs: which [container](../containers/index.md) runs on
which infrastructure, in which environment.

## To fill in when it is built

* Tooling and how to plan / apply it
* Environments (dev, staging, production) and how they differ
* Where each container runs: [Web.Api](../containers/api/web-api.md), [PostgreSQL](../containers/api/postgres.md),
  [Redis](../containers/api/redis.md), [Admin Web](../containers/admin-web/admin-web.md),
  [Customer Web](../containers/customer-web/customer-web.md), [Customer Mobile](../containers/customer-mobile/customer-mobile.md)
* How secrets and configuration reach the containers
