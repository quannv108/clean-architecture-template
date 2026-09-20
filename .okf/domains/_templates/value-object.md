---
type: Template
title: "Value Object Template"
description: "Copy this when adding a value object."
tags: [template, domain, value-object]
status: stable
---

# Value Object Template

> Copy to `.okf/domains/<slice>/<value-object>.md`, replace everything, add it to [Domains](../index.md). A
> SharedKernel value object gets a row in [SharedKernel Layer](../../architecture/components/shared-kernel.md)
> instead of a page. Delete this quote block.

```yaml
---
type: Value Object
title: "Money"
description: "One sentence: what value this represents."
resource: src/SharedKernel/Money/Money.cs
tags: [domain, value-object]
status: draft
---
```

## What it represents

The value, and why it is a type rather than a primitive - what invalid states the type makes unrepresentable.

## Equality components

Which values take part in equality. Two instances with the same components are the same value.

## Creation and validation

The factory, what it normalises, and which [`Error`](../../../src/SharedKernel/Error.cs) each failure returns. An
instance that exists is valid; there is no "validate later".

## Placement

| Used by | Goes in |
|---|---|
| One slice | `src/Domain/<Feature>/` |
| More than one slice | `src/SharedKernel/<Concept>/` with its `*Errors.cs` |

Moving it to SharedKernel the moment a second slice needs it is what prevents cross-slice coupling - see
[SharedKernel Layer](../../architecture/components/shared-kernel.md).

## Persistence

How EF Core stores it - owned type, value converter, or separate columns. If it is encrypted, note that it
cannot be indexed or compared server-side ([`EncryptedString`](../../../src/SharedKernel/EncryptedString.cs)).

## Checklist

- [ ] Derives [`ValueObject`](../../../src/SharedKernel/ValueObject.cs)
- [ ] Immutable; no public setters
- [ ] Private constructor, `Create(...)` returning `Result<T>`
- [ ] `*Errors.cs` beside it
- [ ] Unit tests covering equality and every validation branch
