using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Outbox;

public sealed record GetOutboxStatsQuery(DateTime SinceUtc) : IQuery<GetOutboxStatsResponse>;

public sealed record GetOutboxStatsResponse(IReadOnlyList<OutboxTypeStat> Types);

public sealed record OutboxTypeStat(string Type, IReadOnlyDictionary<OutboxMessageStatus, int> Counts)
{
    public int Total => Counts.Values.Sum();

    public int Count(OutboxMessageStatus status) => Counts.GetValueOrDefault(status);

    public double SuccessRate =>
        Count(OutboxMessageStatus.Processed) + Count(OutboxMessageStatus.Failed) is var terminal && terminal > 0
            ? (double)Count(OutboxMessageStatus.Processed) / terminal
            : 0;
}

internal sealed class GetOutboxStatsQueryHandler(IReadOnlyApplicationDbContext context)
    : IQueryHandler<GetOutboxStatsQuery, GetOutboxStatsResponse>
{
    public async Task<Result<GetOutboxStatsResponse>> Handle(
        GetOutboxStatsQuery query,
        CancellationToken cancellationToken)
    {
        var rows = await context.OutboxMessages
            .AsNoTracking()
            .Where(m => m.OccurredOnUtc >= query.SinceUtc)
            .GroupBy(m => new { m.Type, m.Status })
            .Select(g => new { g.Key.Type, g.Key.Status, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var types = rows
            .GroupBy(r => r.Type)
            .Select(g => new OutboxTypeStat(g.Key, g.ToDictionary(r => r.Status, r => r.Count)))
            .ToList();

        return Result.Success(new GetOutboxStatsResponse(types));
    }
}
