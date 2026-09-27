---
type: Term
title: "ProblemDetails"
description: "The RFC 7807 standard JSON shape for HTTP error responses."
tags: [http, errors, api]
status: stable
---

# ProblemDetails

Every failure response here is ProblemDetails, produced by
[`CustomResults.Problem`](../../api/src/Web.Api/Infrastructure/CustomResults.cs) from an [`Error`](../../api/src/SharedKernel/Error.cs), or by
[`GlobalExceptionHandler`](../../api/src/Web.Api/Infrastructure/GlobalExceptionHandler.cs) for anything unhandled.

The status comes from the error's [`ErrorType`](../../api/src/SharedKernel/Error.cs), so no endpoint writes a status
code by hand and two endpoints cannot disagree about what "not found" means.
