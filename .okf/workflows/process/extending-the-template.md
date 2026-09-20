---
type: Workflow
title: Extending the Template
description: How to take this template into a real product - and how to grow this knowledge base alongside the code so agents stay useful.
tags: [template, onboarding, knowledge-base, extension]
status: stable
---

# Extending the Template

This repository is a starting point. The code ships with three example slices
([AuditLogs](../../domains/audit-logs/audit-logs.md), [Emails](../../domains/emails/emails.md), [Outbox](../../domains/outbox/outbox.md)); your
product's domains do not exist yet. Neither does the knowledge about them - which is what this bundle is
for.

## 1. Rename and reshape

* Rename `CleanArchitecture.slnx`, the root namespace in `Directory.Build.props`, and the database schema
  constants in [`SchemaNameConstants`](../../../src/Infrastructure/Database/SchemaNameConstants.cs).
* Decide which example slices to keep. `Outbox` and `AuditLogs` are infrastructure you almost certainly
  want; `Emails`/`ExampleDomainA` are demonstrations - delete them once you have a real slice.

## 2. Add your first domain

Follow [Add a Feature](../engineering/add-a-feature.md) for the code.

## 3. Record the knowledge in the same commit

This is the part that keeps agents accurate. For each new concept, copy the matching template:

| You added | Create | From |
|---|---|---|
| A business domain or slice | `.okf/domains/<slice>/<slice>.md` (plus an `index.md` listing it) | [Domain Slice Template](../../domains/_templates/domain.md) |
| An entity or aggregate | `.okf/domains/<slice>/<entity>.md` | [Entity Template](../../domains/_templates/entity.md) |
| A value object | `.okf/domains/<slice>/<value-object>.md` | [Value Object Template](../../domains/_templates/value-object.md) |
| A decision you had to argue about | `.okf/adr/NNNN-<slug>.md` | [ADR Template](../../adr/_template-adr.md) |
| A recurring implementation shape | `.okf/engineering/patterns/<name>.md` | [Pattern Template](../../engineering/patterns/_template-pattern.md) |
| A repeatable procedure | `.okf/workflows/<area>/<name>.md` | [Workflow Template](../_template-workflow.md) |
| A word your team uses with a specific meaning | `.okf/glossary/<term>.md` | [Term Template](../../glossary/_template-term.md) |
| A new library or service | `.okf/engineering/technologies/<name>.md` | [Technology Template](../../engineering/technologies/_template-technology.md) |

Then add a line to the directory's `index.md` and an entry to [Knowledge Base Update Log](../../log.md).

## 4. Keep it honest

* **One concept, one file.** If you are writing the same paragraph in two files, one of them should be a
  link.
* **`resource:` points at code.** When the code moves, fix the frontmatter; when the two disagree, the code
  is right.
* **Mark what you have checked.** Add `verified: { by: human:<your-id>, at: <ISO8601> }` to a file once you
  have read it against the code. Everything here started machine-generated and unverified.
* **Deprecate rather than delete.** Set `status: deprecated` and say what replaced it, so an agent following
  an old link learns something instead of hitting a 404.

## 5. Keep the entry point small

`AGENTS.md` and `CLAUDE.md` should stay short enough to read in full every session. New knowledge goes into
`.okf/`, and the entry point gains at most a link. If you find yourself adding a third paragraph to
`AGENTS.md`, it belongs here instead.
