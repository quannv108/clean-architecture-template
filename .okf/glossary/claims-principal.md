---
type: Term
title: "ClaimsPrincipal"
description: "The .NET representation of an authenticated caller and their claims."
tags: [security, authentication]
status: stable
---

# ClaimsPrincipal

Read in Infrastructure by [`UserContext`](../../src/Infrastructure/Authentication/UserContext.cs) through
[`ClaimsPrincipalExtensions`](../../src/Infrastructure/Authentication/ClaimsPrincipalExtensions.cs), and exposed to Application as
[`IUserContext`](../../src/Application/Abstractions/Authentication/IUserContext.cs).

Application and Domain never see it. Handlers ask "who is this?" without any HTTP type crossing the
boundary - and without breaking in background work, where there is no principal at all.
