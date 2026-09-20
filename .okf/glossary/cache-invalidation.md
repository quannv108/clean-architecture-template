---
type: Term
title: "Cache Invalidation"
description: "Removing a cached entry when the underlying data changes."
tags: [caching, correctness]
status: stable
---

# Cache Invalidation

Here it is explicit: after `SaveChangesAsync`, the command handler calls the repository's
`RemoveCacheAsync(...)`.

Forgetting that line is the most common caching bug in this codebase, and it presents as "the update
worked but the API still shows the old value" - which people first blame on the browser.

See [Cached Read](../engineering/patterns/cached-read.md).
