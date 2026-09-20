---
type: Term
title: "Interceptor"
description: "An EF Core hook that runs during operations such as SaveChanges."
tags: [data, ef-core, persistence]
status: stable
---

# Interceptor

Two run here on every save:
[`EntityIdGenerationInterceptor`](../../src/Infrastructure/Database/Interceptors/EntityIdGenerationInterceptor.cs) assigns
[version 7 GUIDs](guid-v7.md), and
[`AuditableEntityInterceptor`](../../src/Infrastructure/Database/Interceptors/AuditableEntityInterceptor.cs) stamps audit fields.

Because they run in the save path, **`ExecuteUpdate` and `ExecuteDelete` skip them entirely** - one of
several reasons those are banned in handlers.
