---
type: Term
title: "Domain-Driven Design (DDD)"
description: "Modelling software around the business domain, with behaviour on entities and a shared vocabulary between code and business."
tags: [architecture, ddd, domain]
status: stable
---

# Domain-Driven Design (DDD)

Behaviour lives on the model rather than in services that manipulate data structures. The vocabulary in the
code is the vocabulary the business uses ([Ubiquitous Language](ubiquitous-language.md)), and the model
is divided into boundaries ([Bounded Context](bounded-context.md)) within which a word means one thing.

Here: entities with factories and behaviour methods, value objects, domain events, and a
[shared kernel](../architecture/components/shared-kernel.md).
