---
type: ADR
title: "ADR 0008: Result<T> for expected failures, exceptions for the unexpected"
description: "Return failures as values so they appear in signatures and map to HTTP statuses, keeping exceptions for genuinely exceptional conditions."
tags: [adr, result, error-handling, exceptions]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0008: Result<T> for expected failures, exceptions for the unexpected

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

"Not found", "already exists", "not allowed in this state" are ordinary outcomes of an operation, not
exceptional conditions. Modelling them as exceptions hides them from the method signature, makes the
control flow invisible, costs stack unwinding on a common path, and scatters `catch` blocks that each decide
a status code slightly differently.

## Decision

Every operation that can fail returns [`Result`](../../src/SharedKernel/Result.cs) or `Result<T>`. Failures carry an
[`Error`](../../src/SharedKernel/Error.cs) with a dotted code and an [`ErrorType`](../../src/SharedKernel/Error.cs);
endpoints call `result.Match(Results.Ok, CustomResults.Problem)` and the status code follows from the type.

Exceptions remain for the genuinely unexpected, caught once by
[`GlobalExceptionHandler`](../../src/Web.Api/Infrastructure/GlobalExceptionHandler.cs) and returned as a 500 with no internal
detail.

## Consequences

**Good.** Failure is visible in the signature. Status mapping lives in one place, so two endpoints cannot
disagree about what "not found" means. Domain code stays free of HTTP concepts while still deciding the
*kind* of failure. Tests assert on error codes rather than exception types.

**Costly.** More ceremony: results have to be checked and propagated rather than thrown past, and a chain of
operations reads less linearly than a chain of throwing calls. `Result<T>.Value` on a failure is a
programming error the type system does not prevent. Library code still throws, so Infrastructure adapters
have to translate at the boundary.

**Rules:** do not throw for an expected failure; do not catch exceptions to turn them into results anywhere
except an Infrastructure adapter; choose the `ErrorType` deliberately, because it *is* the status code.
