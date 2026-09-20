---
type: Term
title: "GUID Version 7"
description: "A time-ordered UUID, used for every entity identifier here."
tags: [data, identifiers, database, performance]
status: stable
---

# GUID Version 7

`Guid.CreateVersion7()` embeds a timestamp in the high bits, so ids sort roughly by creation time.

That matters for indexes: sequential ids append at the right-hand edge of the B-tree, while random version
4 GUIDs scatter inserts and fragment the index.

Assigned on save by
[`EntityIdGenerationInterceptor`](../../src/Infrastructure/Database/Interceptors/EntityIdGenerationInterceptor.cs) - **never set `Id`
yourself**.
