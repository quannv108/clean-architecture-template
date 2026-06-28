using Application.Abstractions.Communication.Email;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Web.Api.Pages.Dev;

internal sealed class EmailModel(IOptions<EmailOptions> emailOptions) : PageModel
{
    public string Provider => emailOptions.Value.Provider ?? "(not set)";

    public string FromAddress => emailOptions.Value.FromAddress ?? "(not set)";

    public bool IsDummyProvider => string.Equals(Provider, "Dummy", StringComparison.OrdinalIgnoreCase);
}
