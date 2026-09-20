---
type: Term
title: "Key Rotation"
description: "Replacing an encryption key without losing access to data encrypted with the old one."
tags: [security, encryption]
status: stable
---

# Key Rotation

Possible here without a data migration because every stored value is prefixed with its
[key version](key-version.md). Increment `CurrentVersion`, add the new key, move the old one to
`LegacyKeys`, deploy. New writes use the new key; old rows keep decrypting with theirs.

**The dangerous step is the last one.** Removing a legacy key before all its rows are re-encrypted makes
them permanently unreadable, and the failure appears whenever someone next reads one.

See [Field Encryption and Key Rotation](../engineering/patterns/encryption.md).
