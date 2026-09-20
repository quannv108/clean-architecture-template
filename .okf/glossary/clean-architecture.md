---
type: Term
title: "Clean Architecture"
description: "An architecture in which dependencies point inward toward the domain, so business rules depend on nothing."
tags: [architecture, ddd]
status: stable
---

# Clean Architecture

Business rules sit at the centre and depend on nothing; frameworks, databases and transports sit at the
edge and depend inward. Anything the centre needs from the edge is expressed as an interface the centre
owns and the edge implements.

Here: [Layered Architecture](../architecture/cross-cutting/layered-architecture.md), enforced by
[Layered Architecture](../architecture/cross-cutting/layered-architecture.md).
