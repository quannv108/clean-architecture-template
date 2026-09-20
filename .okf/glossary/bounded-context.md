---
type: Term
title: "Bounded Context"
description: "A boundary within which a domain term has one unambiguous meaning."
tags: [architecture, ddd, domain]
status: stable
---

# Bounded Context

"Order" means something different to fulfilment than it does to billing. A bounded context is the boundary
inside which one meaning holds; across the boundary, the concepts are translated rather than shared.

In this template a [vertical slice](vertical-slice.md) is the working unit of that boundary. Slices
communicate through [domain events](domain-event.md), cached repositories and shared response DTOs - never
by sharing an entity.
