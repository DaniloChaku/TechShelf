using Ardalis.Specification;
using System.Text.Json.Serialization;
using TechShelf.Domain.Entities;

namespace TechShelf.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductsSortBy
{
    Name,
    Price
}

public static class ISpecificationBuilderExtensions
{
    public static IOrderedSpecificationBuilder<Product> ApplySorting(
        this ISpecificationBuilder<Product> builder, 
        ProductsSortBy sortBy,
        bool isDescending = false)
    {
        return sortBy switch
        {
            ProductsSortBy.Name => isDescending
                ? builder.OrderByDescending(p => p.Name)
                : builder.OrderBy(p => p.Name),
            ProductsSortBy.Price => isDescending
                ? builder.OrderByDescending(p => p.Price)
                : builder.OrderBy(p => p.Price),
            _ => throw new NotImplementedException()
        };
    }
}
