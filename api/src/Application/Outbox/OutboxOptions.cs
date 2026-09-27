using System.ComponentModel.DataAnnotations;

namespace Application.Outbox;

/// <summary>
/// Configuration options for outbox message processing.
/// </summary>
public sealed class OutboxOptions
{
    /// <summary>
    /// Interval, in milliseconds, between consecutive batch polls while draining a backlog.
    /// </summary>
    [Range(100, 600000, ErrorMessage = "Outbox MinPollingIntervalMs must be between 100ms and 600000ms (10 minutes)")]
    public int MinPollingIntervalMs { get; set; } = 1000;

    /// <summary>
    /// Idle poll interval, in milliseconds: how long to wait for new messages before polling again
    /// when the outbox is empty. A signal (new outbox row committed) wakes the processor earlier.
    /// </summary>
    [Range(100, 600000, ErrorMessage = "Outbox MaxPollingIntervalMs must be between 100ms and 600000ms (10 minutes)")]
    public int MaxPollingIntervalMs { get; set; } = 300000;

    /// <summary>
    /// Maximum number of messages to process in a single batch.
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Outbox BatchSize must be between 1 and 1000")]
    public int BatchSize { get; set; } = 20;
}
