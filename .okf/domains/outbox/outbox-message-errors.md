---
type: Domain Errors
title: "OutboxMessageErrors"
description: "Domain error factories for outbox message failures."
resource: src/Domain/Outbox/OutboxMessageErrors.cs
tags: [domain, outbox, errors]
status: stable
---

# OutboxMessageErrors

Error factories for outbox failures - an unknown event type, a payload that will not deserialize, a message
that cannot be marked processed.

Follows the `"{Entity}.{ErrorName}"` code convention and lives beside its entity in `Domain/Outbox/`. See
[Error Codes](../../engineering/conventions/error-codes.md).

A deserialization failure here usually means an event type was renamed or moved after messages referencing
it were already written - worth remembering before renaming a domain event.
