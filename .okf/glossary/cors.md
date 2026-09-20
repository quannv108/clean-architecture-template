---
type: Term
title: "CORS"
description: "Browser-enforced rules about which origins may call the API."
tags: [http, security, browser]
status: stable
---

# CORS

Configured in `Web.Api/Extensions/Cors/`, with policy names as constants and allowed origins from
configuration.

Two things to keep in mind: a misspelled policy name fails quietly rather than loudly, and a permissive
development policy leaking into a deployed environment is the classic CORS incident - which is what named
constants plus per-environment configuration are there to prevent.
