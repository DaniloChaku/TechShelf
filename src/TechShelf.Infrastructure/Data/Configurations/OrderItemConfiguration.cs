using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechShelf.Domain.Orders;

namespace TechShelf.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
                .IsRequired();

        builder.Property(x => x.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.OwnsOne(x => x.ProductOrdered, owned =>
        {
            owned.Property(p => p.ProductId)
                .IsRequired();

            owned.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(OrderConstants.OrderItem.ProductNameMaxLength);

            owned.Property(p => p.ImageUrl)
                .IsRequired()
                .HasMaxLength(OrderConstants.OrderItem.ProductImageUrlMaxLength);
        });
    }
}
