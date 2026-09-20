---
type: Template
title: "Pattern Template"
description: "Copy this when documenting a recurring implementation shape."
tags: [template, pattern]
status: stable
---

# Pattern Template

> Copy to `.okf/engineering/patterns/<slug>.md`, replace everything, add it to [Pattern](index.md). Delete this quote
> block.

```yaml
---
type: Pattern
title: "Pattern Name"
description: "One sentence: what shape this is and what it solves."
resource: src/<where an instance lives>
tags: [topic]
status: draft
---
```

## Problem

What goes wrong without this. Be concrete - a failure someone has actually had.

## How

The rule, as bullets: what to do and why each step matters. This is the knowledge - write it so it holds
for every instance, not just one.

Then **one snippet of only the distinctive lines** (under ~8) and a link to a real instance in `src/` for
the rest. A pattern page is not a place to paste a class; if a reader needs the whole file, the link gives
it to them unabridged and always current.

## Checklist

A tickable list. This is the part people actually use during review, so make each item checkable by looking
at a diff.

## When not to use it

Every pattern has a range. Say where it stops and what to use instead - this section is what stops a good
pattern being applied everywhere until it is a problem.

## Related

The concepts, constraints and ADRs a reader needs next.

---

**A pattern, not a decision.** If you are arguing about *whether* to do something, that is an
[ADR](../../adr/index.md). A pattern documents *how*, once the argument is settled.
