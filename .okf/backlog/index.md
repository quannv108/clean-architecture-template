Known gaps and planned work carried into this knowledge base. This is not a task tracker - it is the set of
things a reader should know are unfinished before they build on top of them.

# Concepts

* [Replace outbox polling with channel-based notification](outbox-channel-notification.md) - Stop polling OutboxMessage on a fixed interval; use a Channel to notify the processor and back off polling when idle.
* [Improve handling of failed outbox messages](outbox-failure-retry.md) - Persist failed messages in a distinct status and add a background service that retries them with back-off for transient failures.
* [Remove the CA1873 suppressions once the analyzer is fixed](remove-ca1873-suppressions.md) - A .NET 10 analyzer regression fires on ordinary logger calls; suppressions are in place across Application and Infrastructure.
* [docs referenced a CodeGenerator tool that is not in the repository](code-generator-missing.md) - The previous docs pointed at tools/CodeGenerator for scaffolding; no such project exists.

The first two came from the previous `docs/PendingTasks.md`; the last two were found while building this
bundle.
