---
type: Template
title: "Term Template"
description: "Copy this when adding a glossary entry."
tags: [template, glossary]
status: stable
---

# Term Template

> Copy to `.okf/glossary/<slug>.md`, replace everything, put the subject first in `tags:` and add the entry
> under that subject's `#` section in [Glossary](index.md). Delete this quote block.

```yaml
---
type: Term
title: "The Term"
description: "One sentence definition."
tags: [subject, topic]     # subject = the # section in index.md: caching, data, http, ...
status: stable
---
```

## Body

Two to six lines. A glossary entry earns its place by being short.

1. **Define it generally** in one sentence.
2. **Say what it means here** - which files, which rule, which choice this codebase made. This is the part
   a general definition cannot give, and the reason the file exists.
3. **Link** to the component, pattern or constraint that owns it.

If it takes more than a screen, it is not a term - it is a [pattern](../engineering/patterns/index.md) or a
[system concept](../architecture/index.md), and the glossary entry should link to that instead.

## When to add one

When a word in a document, a code review or a slice's ubiquitous language would make a new reader pause.
Business terms specific to your product are exactly what this directory is for once the template becomes a
real application - see [Extending the Template](../workflows/process/extending-the-template.md).
