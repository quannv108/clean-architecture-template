using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Communication.Email;

internal sealed class EmailOptions
{
    // Set Email:Provider to "SES" to enable this sender
    public string? Provider { get; set; } // Optional, used to switch providers via config

    // Default From address if EmailMessage.From is null
    [EmailAddress(ErrorMessage = "Email FromAddress must be a valid email address")]
    public string? FromAddress { get; set; }

    public SesEmailOptions Ses { get; set; } = new();
}

internal sealed class SesEmailOptions
{
    // Required when Provider is "SES"
    public string? Region { get; set; }

    // Optional explicit credentials; if not set, SDK default credentials chain is used (env vars/instance role, etc.)
    public string? AccessKeyId { get; set; }
    public string? SecretAccessKey { get; set; }
}
