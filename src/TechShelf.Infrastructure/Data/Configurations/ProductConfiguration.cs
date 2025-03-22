using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechShelf.Domain.Products;

namespace TechShelf.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(ProductConstants.NameMaxLength);

        builder.Property(p => p.Description)
            .HasMaxLength(ProductConstants.DescriptionMaxLength);

        builder.Property(p => p.Price)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(p => p.Stock)
            .IsRequired();

        builder.Property(p => p.ThumbnailUrl)
           .HasMaxLength(ProductConstants.ThumbnailUrlMaxLength);

#pragma warning disable CS8604 // Possible null reference argument.
        builder.Property(p => p.ImageUrls)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                new ValueComparer<List<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList())
            )
            .HasMaxLength(ProductConstants.ImageUrlsMaxLength);
#pragma warning restore CS8604 // Possible null reference argument.

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId);

        builder.HasOne(p => p.Brand)
            .WithMany()
            .HasForeignKey(p => p.BrandId);

        builder.HasIndex(p => p.Name).HasDatabaseName("IX_Product_Name");
        builder.HasIndex(p => p.Price).HasDatabaseName("IX_Product_Price");
    }
}
