using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechShelf.Domain.Orders;

namespace TechShelf.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CustomerId)
            .HasMaxLength(50);

        builder.Property(x => x.PaymentIntentId)
            .HasMaxLength(50);

        builder.Property(x => x.Total)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(a => a.Line1)
                    .IsRequired()
                    .HasMaxLength(100);

            address.Property(a => a.Line2)
                   .HasMaxLength(100)
                   .IsRequired(false);

            address.Property(a => a.City)
                   .IsRequired()
                   .HasMaxLength(50);

            address.Property(a => a.State)
                   .IsRequired()
                   .HasMaxLength(50);

            address.Property(a => a.PostalCode)
                    .IsRequired()
                    .HasMaxLength(10);
        });

        builder.HasMany(x => x.OrderItems)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.History)
            .WithOne()
            .HasForeignKey(h => h.OrderId);

        builder.Navigation(x => x.OrderItems)
            .HasField("_orderItems")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.History)
            .HasField("_history")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
