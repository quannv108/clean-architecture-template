---
type: Template
title: "ADR Template"
description: "Copy this when recording an architecture decision."
tags: [template, adr]
status: stable
---

# ADR Template

> Copy to `.okf/adr/NNNN-<slug>.md` using the next free number, replace everything, add it to
> [Architecture Decision Records](index.md), and add a line to [Knowledge Base Update Log](../log.md). Delete this quote block.

```yaml
---
type: ADR
title: "ADR 0016: Short imperative statement of the decision"
description: "One sentence: what was decided and the shape of the trade-off."
tags: [adr, topic]
status: stable        # draft while under discussion
---
```

**Status:** Proposed | Accepted | Superseded by [ADR NNNN](NNNN-slug.md) - with the date.

## Context

The forces in play: the constraint, the problem, what made this a decision rather than an obvious choice.
Write it so someone in two years understands the situation without you. Keep it factual - no advocacy yet.

## Decision

What was decided, in the present tense. Be specific enough that someone can tell whether code complies.

## Consequences

**Good.** What this buys.

**Costly.** What it costs - and be honest here. An ADR with only benefits is marketing, and the next person
will discover the costs the hard way while assuming they were not considered.

**Alternatives rejected.** Each one with the reason. This is often the most useful section, because the
rejected option is what the next person will propose.

## When to revisit

The condition that would make this worth reopening: a scale threshold, a library changing licence, a
constraint disappearing.

---

## When to write an ADR at all

Write one when the decision is expensive to reverse, when a reasonable engineer would choose otherwise, or
when you have already explained the reasoning twice. Do not write one for a choice with no real
alternatives - that is a [convention](../engineering/conventions/index.md), not a decision.

Supersede rather than edit: an accepted ADR is a record of what was believed at a point in time. Mark the
old one superseded and link forward.
