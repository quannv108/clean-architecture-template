---
type: Term
title: "Permission"
description: "A named capability a caller must hold to invoke an endpoint."
tags: [security, authorization]
status: stable
---

# Permission

Declared per slice as constants in `Application/<Feature>/<Feature>PermissionsConstants.cs` and required by
endpoints through authorization policies.

Constants rather than literals because a mistyped permission name fails open or denies silently, and
because keeping them in Application means a handler and an endpoint cannot disagree about a name.
