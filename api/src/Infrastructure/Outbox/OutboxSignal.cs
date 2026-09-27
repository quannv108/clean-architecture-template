using System.Threading.Channels;

namespace Infrastructure.Outbox;

/// <summary>
/// In-process doorbell that wakes the outbox hosted service as soon as a new outbox row is
/// committed, instead of waiting for the next poll. Purely a latency optimization — correctness
/// rests on the hosted service's fallback poll, so a missed/coalesced signal is never a bug.
/// </summary>
internal sealed class OutboxSignal
{
    private readonly Channel<byte> _channel = Channel.CreateBounded<byte>(
        new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true
        });

    /// <summary>
    /// Notifies the waiter that new work may be available. Best-effort: coalesces bursts into a
    /// single pending signal and cannot meaningfully fail.
    /// </summary>
    public void Notify() => _channel.Writer.TryWrite(default);

    /// <summary>
    /// Waits until a signal arrives or <paramref name="timeout"/> elapses, whichever is first.
    /// </summary>
    public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await _channel.Reader.WaitToReadAsync(linkedCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout elapsed; this is the expected fallback-poll path, not an error.
            return;
        }

        // Drain any pending signal so the next wait starts clean.
        _channel.Reader.TryRead(out _);
    }
}
