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
            .HasMaxLength(OrderConstants.EmailMaxLength);

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(OrderConstants.PhoneNumberMaxLength);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(OrderConstants.FullNameMaxLength);

        builder.Property(x => x.CustomerId)
            .HasMaxLength(OrderConstants.CustomerIdMaxLength);

        builder.Property(x => x.PaymentIntentId)
            .HasMaxLength(OrderConstants.PaymentIntentIdMaxLength);

        builder.Property(x => x.Total)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(a => a.Line1)
                    .IsRequired()
                    .HasMaxLength(OrderConstants.Address.Line1MaxLength);

            address.Property(a => a.Line2)
                   .HasMaxLength(OrderConstants.Address.Line2MaxLength)
                   .IsRequired(false);

            address.Property(a => a.City)
                   .IsRequired()
                   .HasMaxLength(OrderConstants.Address.CityMaxLength);

            address.Property(a => a.State)
                   .IsRequired()
                   .HasMaxLength(OrderConstants.Address.StateMaxLength);

            address.Property(a => a.PostalCode)
                    .IsRequired()
                    .HasMaxLength(OrderConstants.Address.PostalCodeMaxLength);
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

        builder.HasIndex(x => x.Email).HasDatabaseName("IX_Order_Email");
        builder.HasIndex(x => x.PhoneNumber).HasDatabaseName("IX_Order_PhoneNumber");
        builder.HasIndex(x => x.CustomerId).HasDatabaseName("IX_Order_CustomerId");
        builder.HasIndex(x => x.PaymentIntentId).HasDatabaseName("IX_Order_PaymentIntentId");
        builder.HasIndex(x => x.Total).HasDatabaseName("IX_Order_Total");
    }
}
