using TechShelf.Domain.Orders;
using TechShelf.Domain.Orders.ValueObjects;

namespace TechShelf.UnitTests.Domain.Orders;

public class OrderItemTests
{
    [Fact]
    public void InitializesProperties_WhenValidParametersProvided()
    {
        // Arrange
        var productOrdered = new ProductOrdered(1, "Sample Product", "https://example.com/image.jpg");
        var quantity = 2;
        var price = 10.5m;

        // Act
        var orderItem = new OrderItem(productOrdered, quantity, price);

        // Assert
        orderItem.Id.Should().NotBeEmpty();
        orderItem.ProductOrdered.Should().Be(productOrdered);
        orderItem.Quantity.Should().Be(quantity);
        orderItem.Price.Should().Be(price);
    }

    [Fact]
    public void ThrowsArgumentNullException_WhenProductOrderedIsNull()
    {
        // Arrange
        ProductOrdered? productOrdered = null;
        var quantity = 2;
        var price = 10.5m;

        // Act
        Action act = () => new OrderItem(productOrdered!, quantity, price);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithMessage("*productOrdered*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ThrowsArgumentOutOfRangeException_WhenQuantityIsZeroOrNegative(int invalidQuantity)
    {
        // Arrange
        var productOrdered = new ProductOrdered(1, "Sample Product", "https://example.com/image.jpg");
        var price = 10.5m;

        // Act
        Action act = () => new OrderItem(productOrdered, invalidQuantity, price);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithMessage("*quantity*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10.5)]
    public void ThrowsArgumentOutOfRangeException_WhenPriceIsZeroOrNegative(decimal invalidPrice)
    {
        // Arrange
        var productOrdered = new ProductOrdered(1, "Sample Product", "https://example.com/image.jpg");
        var quantity = 2;

        // Act
        Action act = () => new OrderItem(productOrdered, quantity, invalidPrice);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithMessage("*price*");
    }

    [Fact]
    public void GetTotal_ReturnsCorrectValue()
    {
        // Arrange
        var productOrdered = new ProductOrdered(1, "Sample Product", "https://example.com/image.jpg");
        var quantity = 3;
        var price = 15.0m;

        // Act
        var orderItem = new OrderItem(productOrdered, quantity, price);
        var total = orderItem.GetTotal();

        // Assert
        total.Should().Be(45.0m);
    }
}
