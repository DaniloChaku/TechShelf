using TechShelf.Domain.Common;
using TechShelf.Domain.Orders.ValueObjects;

namespace TechShelf.Domain.Orders;

public class OrderItem : Entity<Guid>
{
    public ProductOrdered ProductOrdered { get; private set; }
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private OrderItem() { } // For EF Core
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public OrderItem(ProductOrdered productOrdered, int quantity, decimal price)
    {
        ArgumentNullException.ThrowIfNull(productOrdered);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        Id = Guid.NewGuid();
        ProductOrdered = productOrdered;
        Quantity = quantity;
        Price = price;
    }

    public decimal GetTotal() => Quantity * Price;
}
