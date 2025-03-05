namespace TechShelf.Domain.Orders.ValueObjects;

public record ProductOrdered
{
    public int ProductId { get; private set; }
    public string Name { get; private set; }
    public string ImageUrl { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ProductOrdered() { } // For EF Core
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public ProductOrdered(int productId, string name, string imageUrl)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);

        ProductId = productId;
        Name = name;
        ImageUrl = imageUrl;
    }
}
