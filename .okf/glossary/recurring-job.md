---
type: Term
title: "Recurring Job"
description: "Work executed on a schedule by a job server."
tags: [async, background-jobs, scheduling]
status: stable
---

# Recurring Job

Registered with a cron expression in
[`HangfireRecurringJobConfigurator`](../../src/Infrastructure/BackgroundJobs/Hangfire/HangfireRecurringJobConfigurator.cs). **Not registered
there means it never runs** - there is no discovery by convention.

Every instance runs the schedule, so the [job logic](job-logic.md) should take a
[distributed lock](../engineering/patterns/distributed-lock.md) with `TimeSpan.Zero` and skip when it loses. Otherwise
the work happens N times.
