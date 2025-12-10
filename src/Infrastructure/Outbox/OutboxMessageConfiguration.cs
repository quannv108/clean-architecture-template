using Domain.Outbox;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Extensions;

namespace Infrastructure.Outbox;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable(nameof(OutboxMessage).ToSnakeCase().ToPlural(), SchemaNameConstants.Outbox);

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(o => o.Content)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(o => o.OccurredOnUtc)
            .IsRequired()
            .HasConversion(d => DateTime.SpecifyKind(d, DateTimeKind.Utc), v => v);

        builder.Property(o => o.ProcessedOnUtc)
            .HasConversion(d => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : d, v => v);

        builder.Property(o => o.Error)
            .HasMaxLength(4000);

        builder.Property(o => o.ProcessedByMachine)
            .HasMaxLength(256);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<int>();

        // Index for processing performance
        builder.HasIndex(o => o.ProcessedOnUtc);
    }
}
