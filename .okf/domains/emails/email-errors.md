---
type: Domain Errors
title: "EmailErrors"
description: "Domain error factories for email message failures."
resource: src/Domain/Emails/EmailErrors.cs
tags: [domain, email, errors]
status: stable
---

# EmailErrors

Error factories for [`EmailMessage`](email-message.md) - an invalid address, an empty body, an illegal
status transition.

Choosing the right [`ErrorType`](../../../src/SharedKernel/Error.cs) here is what gives the endpoint its status code
without any mapping code: a bad address is `Validation` (400), an illegal transition is `Conflict` (409).
See [Error Codes](../../engineering/conventions/error-codes.md).
