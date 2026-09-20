---
type: Term
title: "xmin"
description: "The PostgreSQL system column holding the transaction id that last wrote a row, used here as the EF row version."
tags: [data, postgresql, concurrency, ef-core]
status: stable
---

# xmin

Every PostgreSQL row has a hidden `xmin`. Mapping it to `Entity.Version` gives
[optimistic concurrency](optimistic-concurrency.md) with **no extra column and nothing to maintain** -
PostgreSQL updates it itself, so no write path can forget to bump a version.

It is 32 bits and wraps, which is theoretical on all but extreme churn. It also ties the solution to
PostgreSQL. See [ADR 0004: Optimistic concurrency with PostgreSQL xmin](../adr/0004-xmin-optimistic-concurrency.md).
