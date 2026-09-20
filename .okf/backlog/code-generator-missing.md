---
type: Task
title: "docs referenced a CodeGenerator tool that is not in the repository"
description: "The previous docs pointed at tools/CodeGenerator for scaffolding; no such project exists."
tags: [backlog, tooling, documentation, discrepancy]
status: draft
---

# docs referenced a CodeGenerator tool that is not in the repository

## Finding

The previous `docs/VerticalSliceStructure.md` and `docs/FeatureTemplates.md` both referenced a code
generator:

```
dotnet run --project tools/CodeGenerator -- gen-entity -n <EntityName>
```

**There is no `tools/` directory in this repository.** The command does not work, and that reference was
carried in the documentation for some time.

It is recorded here rather than silently dropped because either answer is useful: the tool exists somewhere
and should be restored, or it never shipped and the reference was aspirational.

## Options

1. **Restore or add it.** Scaffolding a slice is a good fit for this architecture, where a feature is a
   predictable set of files - see [Add a Feature](../workflows/engineering/add-a-feature.md).
2. **Confirm it is not coming**, and close this.

Either way, [Add a Feature](../workflows/engineering/add-a-feature.md) is the working procedure today and
makes no mention of a generator.
