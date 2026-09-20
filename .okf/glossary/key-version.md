---
type: Term
title: "Key Version"
description: "The identifier of the encryption key used for a value, stored with the ciphertext."
tags: [security, encryption]
status: stable
---

# Key Version

Stored values take the form `{KeyVersion}:{CipherText}` - for example `2:base64cipher`. The row records
which key produced it, so mixed-version data is normal rather than an error state.

This prefix is the whole reason [key rotation](key-rotation.md) needs no migration.
