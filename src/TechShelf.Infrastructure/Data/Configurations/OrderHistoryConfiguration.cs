using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechShelf.Domain.Entities.OrderAggregate;

namespace TechShelf.Infrastructure.Data.Configurations;

public class OrderHistoryConfiguration : IEntityTypeConfiguration<OrderHistoryEntry>
{
    public void Configure(EntityTypeBuilder<OrderHistoryEntry> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(500);
    }
}
