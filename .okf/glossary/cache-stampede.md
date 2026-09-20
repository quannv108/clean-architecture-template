---
type: Term
title: "Cache Stampede"
description: "Many concurrent requests all missing the same key and hitting the database together."
tags: [caching, performance]
status: stable
---

# Cache Stampede

Handled by [`HybridCache`](../engineering/technologies/hybrid-cache.md), which coalesces concurrent misses for the same
key so only one factory call runs.

Worth knowing because it removes a reason people sometimes give for hand-rolling cache code - the framework
already does it.
