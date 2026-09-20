---
type: Template
title: "Convention Template"
description: "Copy this when recording a naming, placement or style rule."
tags: [template, convention]
status: stable
---

# Convention Template

> Copy to `.okf/engineering/conventions/<slug>.md`, replace everything, add it to [Convention](index.md). Delete this quote
> block.

```yaml
---
type: Convention
title: "What the convention governs"
description: "One sentence stating the rule."
tags: [topic]
status: stable
---
```

## The rule

State it in one or two lines, at the top, before any explanation. Someone checking a diff should get their
answer without scrolling.

## Examples

Good and bad side by side. A bad example with a one-line note on what is wrong teaches faster than a
paragraph of prose.

## Why

Short. A convention with a reason survives the first time it is inconvenient; one without gets treated as
taste and quietly dropped.

## Enforcement

Say whether an architecture test, an analyzer, `.editorconfig` or review enforces this. A convention nothing
enforces will drift - if it matters, consider adding a test in
[`ArchitectureTests`](../../architecture/delivery/test-architecture.md).

---

**A convention, not a decision.** If there were real alternatives worth weighing, it is an
[ADR](../../adr/index.md). A convention is a choice that mainly needed *a* answer, consistently applied.
