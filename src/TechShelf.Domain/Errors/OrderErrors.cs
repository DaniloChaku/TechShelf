using ErrorOr;

namespace TechShelf.Domain.Errors;

public static class OrderErrors
{
    public static Error OrderNotFound(Guid orderId) =>
        Error.NotFound(
            $"Order with ID {orderId} was not found.");
    public static Error InvalidProductInBasket(int productId) =>
        Error.Validation(
            "Order.InvalidProduct",
            $"Cannot create order: product with ID {productId} does not exist.");
    public static Error NotEnoughStock(string productName, int stock) =>
        Error.Validation(
            "Order.NotEnoughStock",
            $"Cannot create order: product {productName} has only {stock} items in stock.");
}
