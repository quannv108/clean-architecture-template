using SharedKernel;

namespace Domain.Emails.Messages;

public sealed class EmailMessage : Entity
{
    public string To { get; private set; }
    public string Subject { get; private set; }
    public string Body { get; private set; }
    public bool IsHtml { get; private set; }
    public string? From { get; private set; }
    public List<string> Cc { get; private set; }
    public List<string> Bcc { get; private set; }
    public Dictionary<string, string> Headers { get; private set; }
    public EmailMessageStatus Status { get; private set; }

    private EmailMessage(string to, string subject, string body, bool isHtml,
        string? from, List<string>? cc, List<string>? bcc,
        Dictionary<string, string>? headers)
    {
        Id = EntityIdGenerator.NewId();
        To = to;
        Subject = subject;
        Body = body;
        IsHtml = isHtml;
        From = from;
        Cc = cc ?? [];
        Bcc = bcc ?? [];
        Headers = headers ?? [];
        Status = EmailMessageStatus.Pending;
    }

    public static EmailMessage Create(string to, string subject, string body, bool isHtml = true,
        string? from = null, List<string>? cc = null, List<string>? bcc = null,
        Dictionary<string, string>? headers = null)
    {
        var message = new EmailMessage(to, subject, body, isHtml, from, cc, bcc, headers);
        return message;
    }

    public void MarkAsSent()
    {
        if (Status != EmailMessageStatus.Pending)
        {
            return;
        }

        Status = EmailMessageStatus.Sent;
        Raise(new EmailSentDomainEvent(Id));
    }

    public void MarkAsFailed(string reason)
    {
        if (Status != EmailMessageStatus.Pending)
        {
            return;
        }

        Status = EmailMessageStatus.Failed;
    }
}
