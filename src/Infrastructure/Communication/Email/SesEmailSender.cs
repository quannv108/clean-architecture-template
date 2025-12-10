using Application.Abstractions.Communication.Email;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Domain.Emails.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Infrastructure.Communication.Email;

internal sealed class SesEmailSender : IEmailSender
{
    private readonly ILogger<SesEmailSender> _logger;
    private readonly EmailOptions _options;
    private readonly AmazonSimpleEmailServiceV2Client _client;

    public SesEmailSender(ILogger<SesEmailSender> logger, IOptions<EmailOptions> options)
    {
        _logger = logger;
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.Ses.Region))
        {
            throw new ArgumentException("Amazon SES Region must be configured (Email:AmazonSES:Region)");
        }

        var regionEndpoint = RegionEndpoint.GetBySystemName(_options.Ses.Region);
        _client = CreateClient(regionEndpoint, _options);
    }

    private static AmazonSimpleEmailServiceV2Client CreateClient(RegionEndpoint region, EmailOptions opts)
    {
        // Prefer explicit keys if both are provided; fall back to default credentials chain.
        if (!string.IsNullOrWhiteSpace(opts.Ses.AccessKeyId) && !string.IsNullOrWhiteSpace(opts.Ses.SecretAccessKey))
        {
            var creds = new BasicAWSCredentials(opts.Ses.AccessKeyId, opts.Ses.SecretAccessKey);
            return new AmazonSimpleEmailServiceV2Client(creds, region);
        }

        return new AmazonSimpleEmailServiceV2Client(region);
    }

    public async Task<Result> SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            var from = !string.IsNullOrWhiteSpace(emailMessage.From) ? emailMessage.From : _options.FromAddress;
            if (string.IsNullOrWhiteSpace(from))
            {
                return Result.Failure(Error.Failure("email.sender.from.is.missing",
                    "No From address provided and Email:AmazonSES:FromAddress is not configured."));
            }

            var destination = new Destination
            {
                ToAddresses = new List<string> { emailMessage.To }
            };

            if (emailMessage.Cc is { Count: > 0 })
            {
                destination.CcAddresses = emailMessage.Cc;
            }

            if (emailMessage.Bcc is { Count: > 0 })
            {
                destination.BccAddresses = emailMessage.Bcc;
            }

            var content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content
                    {
                        Data = emailMessage.Subject
                    },
                    Body = new Body
                    {
                        Html = emailMessage.IsHtml ? new Content { Data = emailMessage.Body } : null,
                        Text = !emailMessage.IsHtml ? new Content { Data = emailMessage.Body } : null
                    }
                }
            };

            var request = new SendEmailRequest
            {
                FromEmailAddress = from,
                Destination = destination,
                Content = content
            };

            var response = await _client.SendEmailAsync(request, cancellationToken).ConfigureAwait(false);

            if ((int)response.HttpStatusCode >= 200 && (int)response.HttpStatusCode < 300)
            {
                return Result.Success();
            }

            _logger.LogError("SES SendEmail failed with status code {StatusCode}", response.HttpStatusCode);
            return Result.Failure(Error.Failure("email.ses.send.failed",
                $"SES SendEmail failed with status code {response.HttpStatusCode}"));
        }
        catch (MessageRejectedException ex)
        {
            _logger.LogError(ex, "SES message rejected: {Message}", ex.Message);
            return Result.Failure(Error.Failure("email.ses.rejected", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SES SendEmail threw exception");
            return Result.Failure(Error.Failure("email.ses.exception", ex.Message));
        }
    }
}
