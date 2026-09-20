---
type: Term
title: "Response DTO"
description: "The read-side shape returned by queries and cached repositories."
tags: [application, dto, read, api]
status: stable
---

# Response DTO

A positional record named `<Operation>Response` or `<Feature>Response`, containing exactly what the caller
needs.

It is also the sanctioned way for one slice to read another's data - slice A takes slice B's
`IBCachedRepository` and receives `BResponse`, never `B` the entity. That is what keeps a read across
slices from becoming a shared entity.
