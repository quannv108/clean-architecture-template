---
type: Term
title: "Entity"
description: "An object with identity that persists across state changes."
tags: [architecture, ddd, domain]
status: stable
---

# Entity

Identified by its id, not by its values - two entities with identical properties are different if their ids
differ. Contrast a [value object](value-object.md).

Here every entity inherits [`Entity`](../../src/SharedKernel/Entity.cs) or
[`AuditedEntity`](../../src/SharedKernel/AuditedEntity.cs), is constructed through a static `Create` factory
returning `Result<T>`, and changes state only through behaviour methods.
