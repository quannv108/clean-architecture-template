using Application.Abstractions.Messaging;
using Application.Outbox;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Api.Pages.Dev;

internal sealed class OutboxTypeModel(IQueryHandler<GetOutboxTypeErrorsQuery, GetOutboxTypeErrorsResponse> handler)
    : PageModel
{
    [BindProperty(SupportsGet = true)] public string Type { get; set; } = string.Empty;

    public IReadOnlyList<OutboxErrorDetail> Errors { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Type))
        {
            return;
        }

        var result = await handler.Handle(new GetOutboxTypeErrorsQuery(Type), cancellationToken);

        Errors = result.Value.Errors;
    }
}
