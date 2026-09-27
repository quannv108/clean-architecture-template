using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Outbox;

public sealed record GetOutboxTypeErrorsQuery(string Type, int Take = 5) : IQuery<GetOutboxTypeErrorsResponse>;

public sealed record GetOutboxTypeErrorsResponse(IReadOnlyList<OutboxErrorDetail> Errors);

public sealed record OutboxErrorDetail(
    Guid Id,
    string Type,
    DateTime OccurredOnUtc,
    string? ProcessedByMachine,
    string? Error,
    string Content);

internal sealed class GetOutboxTypeErrorsQueryHandler(IReadOnlyApplicationDbContext context)
    : IQueryHandler<GetOutboxTypeErrorsQuery, GetOutboxTypeErrorsResponse>
{
    public async Task<Result<GetOutboxTypeErrorsResponse>> Handle(
        GetOutboxTypeErrorsQuery query,
        CancellationToken cancellationToken)
    {
        var errors = await context.OutboxMessages
            .AsNoTracking()
            .Where(m => m.Type == query.Type && m.Status == OutboxMessageStatus.Failed)
            .OrderByDescending(m => m.OccurredOnUtc)
            .Take(query.Take)
            .Select(m => new OutboxErrorDetail(m.Id, m.Type, m.OccurredOnUtc, m.ProcessedByMachine, m.Error, m.Content))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetOutboxTypeErrorsResponse(errors));
    }
}
