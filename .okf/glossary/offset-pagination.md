---
type: Term
title: "Offset Pagination"
description: "Paging with skip and take."
tags: [http, paging, api]
status: stable
---

# Offset Pagination

Simple, and adequate for small or stable result sets. Two problems at scale: the database must scan past
every skipped row, and on a table receiving inserts the pages shift under the reader, so rows are missed or
repeated.

Prefer [cursor pagination](cursor-pagination.md) for append-heavy or deep result sets. Either way, list
endpoints return [`PagedList<T>`](../../src/Web.Api/Endpoints/Generic/PagedList.cs) - a bare array makes adding paging later a
breaking change.
