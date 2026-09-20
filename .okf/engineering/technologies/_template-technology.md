---
type: Template
title: "Technology Template"
description: "Copy this when adding an external framework, library or service."
tags: [template, technology]
status: stable
---

# Technology Template

> Copy to `.okf/engineering/technologies/<slug>.md`, replace everything, add it to [Technology](index.md). Delete this quote
> block.

```yaml
---
type: Technology
title: "Library or Service Name"
description: "One sentence: what it does here."
resource: https://its-homepage
tags: [technology, topic]
status: stable
---
```

## What it is used for **here**

Not what the library does in general - what this codebase uses it for, and which files. Link the source
files that wrap it (relative path, e.g. `../../../src/Infrastructure/<File>.cs`).

## How it is wrapped

Which abstraction hides it, so a reader knows whether its types may appear in their code. In this codebase
the answer is almost always "an interface in `Application/Abstractions/`, implemented by an adapter in
Infrastructure" - see
[Naming and Placement](../conventions/naming.md).

## Configuration

The options class and configuration section. Never put credentials in a committed file.

## Gotchas

The things that cost somebody an afternoon: version-specific bugs, environment variables, sandbox
restrictions, rate limits, behaviour that differs between local and deployed.

## Related

The ADR that chose it, if there is one.

---

**Adding a dependency is a decision.** If a reasonable engineer would have chosen differently, write an
[ADR](../../adr/index.md) as well.
