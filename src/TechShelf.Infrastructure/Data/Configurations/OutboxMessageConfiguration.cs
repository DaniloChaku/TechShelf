using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechShelf.Infrastructure.Data.Outbox;

namespace TechShelf.Infrastructure.Data.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.Type)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.OccurredOn)
            .IsRequired();

        builder.Property(m => m.ProcessedOn)
            .IsRequired(false);

        builder.Property(m => m.RetryCount)
            .IsRequired();

        builder.Property(m => m.Error)
            .IsRequired(false);

        builder.HasIndex(m => m.ProcessedOn);
        builder.HasIndex(m => m.OccurredOn);
        builder.HasIndex(m => m.RetryCount);
    }
}
