---
type: Term
title: "Value Converter"
description: "An EF Core translation between a property's CLR type and its stored representation."
tags: [data, ef-core, persistence]
status: stable
---

# Value Converter

Used here for [`EncryptedString`](../../src/SharedKernel/EncryptedString.cs) (encrypt on write, decrypt on read) and
for storing enums as strings.

Because conversion happens in the provider, the **stored** value is what the database sees. An encrypted
column therefore cannot be indexed, sorted or compared server-side.
