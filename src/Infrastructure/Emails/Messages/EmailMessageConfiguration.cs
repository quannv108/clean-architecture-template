using Domain.Emails.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Emails.Messages;

internal sealed class EmailMessageConfiguration : IEntityTypeConfiguration<EmailMessage>
{
    public void Configure(EntityTypeBuilder<EmailMessage> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.To)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Body)
            .IsRequired();

        builder.Property(e => e.IsHtml)
            .IsRequired();

        builder.Property(e => e.From)
            .HasMaxLength(256);

        builder.Property(e => e.Cc)
            .HasColumnType("jsonb");

        builder.Property(e => e.Bcc)
            .HasColumnType("jsonb");

        builder.Property(e => e.Headers)
            .HasColumnType("jsonb");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.HasIndex(e => e.Status);
    }
}
