---
type: Term
title: "Repository"
description: "A class that mediates access to persisted data for one slice."
tags: [application, data-access, patterns]
status: stable
---

# Repository

Here the term means one specific thing: a **read-side** [cached repository](../../src/Application)
in `Application/<Feature>/Data/`, returning DTOs.

There is **no write-side repository**. Command handlers inject
[`IApplicationDbContext`](../../src/Application/Abstractions/Data/IApplicationDbContext.cs) directly, because `DbContext` is already
a [unit of work](unit-of-work.md) and wrapping it adds indirection without adding a seam.
