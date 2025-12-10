using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Outbox;

/// <summary>
/// Configuration options for outbox message processing.
/// </summary>
internal sealed class OutboxOptions
{
    /// <summary>
    /// Polling interval in milliseconds for checking new outbox messages.
    /// </summary>
    [Range(100, 60000, ErrorMessage = "Outbox PollingIntervalMs must be between 100ms and 60000ms (1 minute)")]
    public int PollingIntervalMs { get; set; } = 5000;

    /// <summary>
    /// Maximum number of messages to process in a single batch.
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Outbox BatchSize must be between 1 and 1000")]
    public int BatchSize { get; set; } = 20;
}
