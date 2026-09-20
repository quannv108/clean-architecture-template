---
type: ADR
title: "ADR 0013: DataAnnotations for shape validation, domain errors for business rules"
description: "Validate command shape with DataAnnotations in a decorator, and leave rules that need data to the domain."
tags: [adr, validation, dataannotations, fluentvalidation]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0013: DataAnnotations for shape validation, domain errors for business rules

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

FluentValidation is the usual choice and is more expressive. It is also another dependency, another set of
classes per command, and another place to look when asking "what makes this request valid".

More importantly, expressive validators invite a real mistake: putting business rules in the validator,
where they run outside the domain, outside the transaction, and duplicate logic the entity should own.

## Decision

Two tiers with a sharp line.

* **Shape** - required, length, range, format - is declared as DataAnnotations attributes on the command
  record and run by [`ValidationDecorator`](../../src/Application/Abstractions/Behaviors/ValidationDecorator.cs) before the handler.
  Custom shape rules that fit the attribute model live in `Application/Abstractions/Validation/` (see
  [`RegularIdAttribute`](../../src/Application/Abstractions/Validation/RegularIdAttribute.cs)).
* **Business rules** - anything needing the database or domain state - live in the handler or the entity
  and return an [`Error`](../../src/SharedKernel/Error.cs).

## Consequences

**Good.** No extra dependency, and the rules are visible on the command itself. The line between "malformed
request" (400) and "request contradicts state" (409) is structural rather than a judgement call. Fewer files
per use case.

**Costly.** DataAnnotations cannot express conditional or cross-property rules cleanly; those become handler
code, which is more verbose than a fluent validator would be. Error messages are less controllable.

**This is why commands use standard, non-positional record syntax** - positional parameters cannot carry
property attributes. See [Record Syntax](../engineering/conventions/record-syntax.md).
