---
type: ADR
title: "ADR 0015: Replace docs/ with an OKF knowledge base at .okf/"
description: "Decompose prose documentation into one concept per file in Open Knowledge Format, and reduce the agent entry point to a pointer."
tags: [adr, documentation, okf, agents, knowledge-base]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0015: Replace docs/ with an OKF knowledge base at .okf/

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

The previous `docs/` tree was twelve long documents. It was written for a human reading top to bottom, and
it had the problems that shape has when agents are the main readers:

* **Concepts had no address.** "The decorator pipeline" was a section inside `Architecture.md`. Nothing
  could link to it precisely, and finding it meant reading the file.
* **Content duplicated and drifted.** Layer dependencies appeared in three files; naming conventions in two.
* **Reading cost was all-or-nothing.** Answering a small question meant loading a 300-line document.
* **`CLAUDE.md` was accreting rules** that belonged in the documentation, because it was the file agents
  reliably read.
* **Decisions were invisible.** The docs said what to do and rarely why, so every rule was equally
  negotiable-looking.

## Decision

Adopt [Open Knowledge Format](https://github.com/GoogleCloudPlatform/open-knowledge-format) v0.2
as `.okf/`: markdown files with YAML frontmatter, one concept per file, addressable by path, organised into
`system/`, `component/`, `domain/`, `adr/`, `pattern/`, `convention/`, `constraint/`, `workflow/`,
`technology/`, `glossary/` and `backlog/`, each with an `index.md` for progressive disclosure. (The
directories were later regrouped under `architecture/` and `engineering/`, and on 2026-09-20 the bundle was
brought into strict conformance with the spec's reserved-file rules — see
[Maintain the Knowledge Base](../workflows/process/maintain-the-knowledge-base.md).)

`docs/` is removed. `AGENTS.md` becomes the minimal entry point and `CLAUDE.md` imports it; both do little
more than point at `.okf/index.md`. `.claude/rules/*.md` reference concepts here rather than restating them.

Every noun in the old documentation has a file, and templates (`_template-*.md`) exist so a project built
from this template grows its knowledge base alongside its code.

## Consequences

**Good.** An agent can read one index and two concept files instead of a whole document. Every rule has a
stable path to link to. Duplication becomes a visible smell, because the same fact in two files is two
files. Decisions have somewhere to live, so "why" travels with "what". The format is plain markdown with no
runtime, readable on GitHub and portable to any tool.

**Costly.** Many more files, which is a larger diff to review and more places to let rot. Cross-links must
be maintained - a moved file breaks links silently unless checked. Contributors have to learn where things
go, which is what the templates and this index are for. And a knowledge base only pays off if it is updated
in the same commit as the code; if that habit does not take, it becomes confidently wrong documentation,
which is worse than none.

**Mitigation.** Keep `AGENTS.md` small enough to read every session, make the frontmatter `resource:` the
link back to code, and treat a stale concept file as a defect. See
[Extending the Template](../workflows/process/extending-the-template.md).
