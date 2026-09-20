---
type: Term
title: "L1 Cache"
description: "The in-memory cache tier, local to one process."
tags: [caching, performance]
status: stable
---

# L1 Cache

Sub-millisecond, and always present. Not shared: without [L2](l2-cache.md), another instance can serve
stale data until its own L1 entry expires.

That staleness window is the reason cache expiry here is short for changing data (2-5 minutes) and longer
only for genuinely stable data. See [Caching Tiers](../architecture/cross-cutting/caching-tiers.md).
