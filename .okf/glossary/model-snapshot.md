---
type: Term
title: "Model Snapshot"
description: "The generated file recording EF Core's current view of the model, used to diff the next migration."
tags: [data, ef-core, migrations]
status: stable
---

# Model Snapshot

`ApplicationDbContextModelSnapshot.cs`. Each new migration is the difference between this snapshot and the
current model.

**This is why you never delete a migration file by hand.** `dotnet ef migrations remove` also reverts the
snapshot; deleting the file alone leaves the snapshot ahead of reality, and the next migration generates an
incorrect diff - usually discovered later, against data.
