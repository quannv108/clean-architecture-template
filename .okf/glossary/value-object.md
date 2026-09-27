---
type: Term
title: "Value Object"
description: "An object defined by its values, with no identity."
tags: [architecture, ddd, domain]
status: stable
---

# Value Object

Two instances with the same components are the same value. Immutable, validated in its factory, so an
instance that exists is always valid.

Placement is the recurring question: one slice needs it -> `Domain/<Feature>/`; more than one ->
`SharedKernel/<Concept>/`. See [`ValueObject`](../../api/src/SharedKernel/ValueObject.cs) and
[`PhoneNumber`](../../api/src/SharedKernel/PhoneNumbers/PhoneNumber.cs).
