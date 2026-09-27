using Application.Abstractions.Messaging;
using Application.Outbox;
using Domain.Outbox;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Api.Pages.Dev;

internal sealed class OutboxModel(IQueryHandler<GetOutboxStatsQuery, GetOutboxStatsResponse> handler) : PageModel
{
    [BindProperty(SupportsGet = true)] public string Window { get; set; } = "24h";

    [BindProperty(SupportsGet = true)] public string? Q { get; set; }

    [BindProperty(SupportsGet = true)] public bool ErrorsOnly { get; set; }

    [BindProperty(SupportsGet = true)] public string Sort { get; set; } = "total";

    public IReadOnlyList<OutboxTypeStat> TypeStats { get; private set; } = [];

    public int TotalProcessed { get; private set; }
    public int TotalFailed { get; private set; }
    public int TotalInFlight { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var sinceUtc = DateTime.UtcNow.AddDays(Window == "7d" ? -7 : -1);

        var result = await handler.Handle(new GetOutboxStatsQuery(sinceUtc), cancellationToken);

        IEnumerable<OutboxTypeStat> types = result.Value.Types;

        if (!string.IsNullOrWhiteSpace(Q))
        {
            types = types.Where(t => t.Type.Contains(Q, StringComparison.OrdinalIgnoreCase));
        }

        if (ErrorsOnly)
        {
            types = types.Where(t => t.Count(OutboxMessageStatus.Failed) > 0);
        }

        types = Sort == "rate"
            ? types.OrderBy(t => t.SuccessRate)
            : types.OrderByDescending(t => t.Total);

        TypeStats = types.ToList();

        TotalProcessed = TypeStats.Sum(t => t.Count(OutboxMessageStatus.Processed));
        TotalFailed = TypeStats.Sum(t => t.Count(OutboxMessageStatus.Failed));
        TotalInFlight = TypeStats.Sum(t =>
            t.Count(OutboxMessageStatus.Pending) + t.Count(OutboxMessageStatus.Processing));
    }
}
