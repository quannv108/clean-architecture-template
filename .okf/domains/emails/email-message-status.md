---
type: Value Object
title: "EmailMessageStatus"
description: "The lifecycle states of an email message, stored as a string."
resource: src/Domain/Emails/EmailMessage.cs
tags: [domain, email, enum]
status: stable
---

# EmailMessageStatus

The status of an [`EmailMessage`](email-message.md) - pending, sent, and whatever failure states your
product needs.

Stored **as a string**, by the convention in
[`BaseApplicationDbContext`](../../../src/Infrastructure/Database/BaseApplicationDbContext.cs). That is deliberate: with ordinal
storage, inserting a value in the middle of the enum silently remaps every existing row.

Transitions belong in behaviour methods on the entity with a guard, never in a handler assigning the
property.
