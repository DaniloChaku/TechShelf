﻿using Ardalis.Specification;
using System.Text.Json.Serialization;
using TechShelf.Domain.Entities;

namespace TechShelf.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductsSortOption
{
    Alphabetically,
    PriceAsc,
    PriceDesc
}

public static class ISpecificationBuilderExtensions
{
    public static IOrderedSpecificationBuilder<Product> ApplySorting(
        this ISpecificationBuilder<Product> builder,
        ProductsSortOption sortBy)
    {
        return sortBy switch
        {
            ProductsSortOption.Alphabetically =>
                builder.OrderBy(p => p.Name),
            ProductsSortOption.PriceAsc =>
                builder.OrderBy(p => p.Price),
            ProductsSortOption.PriceDesc =>
                builder.OrderByDescending(p => p.Price),
            _ => throw new NotImplementedException()
        };
    }
}
