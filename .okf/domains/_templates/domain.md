---
type: Template
title: "Domain Slice Template"
description: "Copy this when adding a business domain or slice to a project built from this template."
tags: [template, domain, slice]
status: stable
---

# Domain Slice Template

> Copy to `.okf/domains/<slice>/<slice>.md`, replace everything, list it in the slice's `index.md` and the slice in [Domains](../index.md), and add a line to
> [Knowledge Base Update Log](../../log.md). Delete this quote block.

```yaml
---
type: Domain Slice
title: "Orders"
description: "One sentence: what business capability this slice owns."
resource: src/Domain/Orders
tags: [domain, orders]
status: draft
---
```

## Purpose

What business capability this slice owns, in two or three sentences. Write it for someone who knows
software but not your business.

## Boundary

What is **in** this slice and what is deliberately **out**. This is the most valuable section and the one
most often skipped - it is what stops the slice absorbing its neighbours over the next year.

## Across the layers

| Layer | Files |
|---|---|
| Domain | entity, errors, domain events |
| Application | handlers, cached repository, event handlers, permissions constants |
| Infrastructure | EF configuration, seeder |
| Web.Api | endpoints |
| Tests | unit and integration test folders |

## Ubiquitous language

The words this slice uses and exactly what each means here. Give each one a file in
[Glossary](../../glossary/index.md) when the meaning is specific to your business, and link it.

## Rules and invariants

What must always be true. Say where each is enforced - an entity guard, a validation attribute, a database
constraint, an architecture test.

## Interactions with other slices

Only through the three doors: domain events, cached repositories, shared response DTOs. Name which, and
why. See [Vertical Slice Architecture](../../architecture/cross-cutting/vertical-slice-architecture.md).

## Open questions

Things not yet decided. Promote each to an [ADR](../../adr/index.md) once it is.
