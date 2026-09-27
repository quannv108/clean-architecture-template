using System.ComponentModel.DataAnnotations;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Validation;
using Domain.AuditLogs;
using SharedKernel;

namespace Application.AuditLogs;

public sealed record CreateAuditLogCommand : ICommand<Guid>, IValidatableObject
{
    [Required(ErrorMessage = "User ID is required")]
    [RegularId]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Action name is required")]
    [StringLength(200, ErrorMessage = "Action name cannot exceed 200 characters")]
    public string ActionName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Action date time is required")]
    public DateTime ActionDateTime { get; set; }

    [Required(ErrorMessage = "URL path is required")]
    public Uri UrlPath { get; set; } = null!;

    [StringLength(45, ErrorMessage = "IP address cannot exceed 45 characters")]
    public string? IpAddress { get; set; }

    public int? HttpResponseCode { get; set; }

    [StringLength(2000, ErrorMessage = "Additional data cannot exceed 2000 characters")]
    public string? AdditionalData { get; set; }

    [Required(ErrorMessage = "Tenant ID is required")]
    public Guid TenantId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UserId == Guid.Empty)
        {
            yield return new ValidationResult(
                "User ID cannot be empty",
                new[] { nameof(UserId) });
        }

        if (ActionDateTime == default)
        {
            yield return new ValidationResult(
                "Action date time cannot be default",
                new[] { nameof(ActionDateTime) });
        }

        if (ActionDateTime > DateTime.UtcNow.AddMinutes(5))
        {
            yield return new ValidationResult(
                "Action date time cannot be in the future",
                new[] { nameof(ActionDateTime) });
        }

        if (UrlPath is null)
        {
            yield return new ValidationResult(
                "URL path cannot be null",
                new[] { nameof(UrlPath) });
        }
    }
}

internal sealed class CreateAuditLogCommandHandler(
    IApplicationDbContext context)
    : ICommandHandler<CreateAuditLogCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAuditLogCommand command, CancellationToken cancellationToken)
    {
        Result<AuditLog> createResult = AuditLog.Create(
            command.UserId,
            command.ActionName,
            command.ActionDateTime,
            command.UrlPath,
            command.IpAddress,
            command.HttpResponseCode,
            command.AdditionalData,
            command.TenantId);

        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Error);
        }

        AuditLog auditLog = createResult.Value;

        context.AuditLogs.Add(auditLog);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(auditLog.Id);
    }
}
