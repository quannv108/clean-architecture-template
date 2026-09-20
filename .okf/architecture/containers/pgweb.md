---
type: Container
title: "pgweb"
description: "A browser UI over the development database, started with the local stack."
tags: [container, c4, database, local-development]
status: stable
---

# pgweb

A web front end for [PostgreSQL](postgres.md), started by [AppHost](apphost.md). Useful for checking what a
migration actually produced, or why an outbox row is still pending.

**Development only**, and read-only in spirit: writing through it bypasses domain behaviour, domain events
and the [audit trail](../../engineering/patterns/audit-logging.md) entirely, so a row changed here is a row nothing
knows about. Remember too that it does **not** apply the soft-delete query filter — rows you see here may
be invisible to the application.

For inspecting the asynchronous machinery, the [dev pages](../../../src/Web.Api/Pages/Dev) are usually a
better tool.
