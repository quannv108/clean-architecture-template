using Domain.AuditLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.AuditLogs;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.ActionName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.ActionDateTime)
            .IsRequired();

        builder.Property(a => a.UrlPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(a => a.HttpResponseCode);

        builder.Property(a => a.AdditionalData)
            .HasMaxLength(2000);

        // Add index for performance on common queries
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.ActionDateTime);
        builder.HasIndex(a => new { a.UserId, a.ActionDateTime });

        builder.Property(a => a.TenantId);
    }
}
