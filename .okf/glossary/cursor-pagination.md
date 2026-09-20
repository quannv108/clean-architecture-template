---
type: Term
title: "Cursor Pagination"
description: "Paging by a position marker rather than a row offset."
tags: [http, paging, api, performance]
status: stable
---

# Cursor Pagination

Used by [`GetAuditLogsQuery`](../../src/Application/AuditLogs/GetAuditLogsQuery.cs), with `ActionDateTime` as the cursor.

Against an append-heavy table, [offset paging](offset-pagination.md) skips and repeats rows as new ones
arrive between page requests. A cursor is stable regardless of inserts, and stays fast at depth because the
database seeks rather than counting past rows.
