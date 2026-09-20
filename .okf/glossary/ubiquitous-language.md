---
type: Term
title: "Ubiquitous Language"
description: "The shared vocabulary of the business, used unchanged in code, tests and conversation."
tags: [architecture, ddd, naming]
status: stable
---

# Ubiquitous Language

If the business says "confirm an order", the method is `Confirm()`, not `UpdateStatusToConfirmed`. The
value of the language is that a conversation and a code review can use the same words without translation -
which is lost the moment code invents its own synonyms.

Record your slice's terms in [Domain Slice Template](../domains/_templates/domain.md) and give each a
file here.
