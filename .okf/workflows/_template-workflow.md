---
type: Template
title: "Workflow Template"
description: "Copy this when documenting a repeatable procedure."
tags: [template, workflow]
status: stable
---

# Workflow Template

> Copy to `.okf/workflows/<area>/<slug>.md` (`engineering/` or `process/`), replace everything, add it to that area's `index.md`. Delete this quote
> block.

```yaml
---
type: Workflow
title: "Do the thing"
description: "One sentence: what this procedure accomplishes."
tags: [workflow, topic]
status: stable
---
```

## Prerequisites

What must be true before starting. Say how to check, not just what is required.

## Steps

Numbered, each one action. Put the exact command in a code block - a described command gets typed wrong.

Call out the flags and steps that are easy to forget, and say what happens when they are missed. "Without
`--output-dir` EF generates into the wrong namespace and the migrations are never discovered" is what makes
someone remember; "remember `--output-dir`" is not.

## Verify

How to know it worked. Include the check that catches the most common mistake.

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|

Write this section from failures that have actually happened. It is the part people arrive at this file
needing.

## Related

Links to the concepts behind the procedure, for the reader who wants to know why rather than how.
