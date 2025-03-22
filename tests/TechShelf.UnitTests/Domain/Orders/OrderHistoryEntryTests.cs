using TechShelf.Domain.Orders;
using TechShelf.Domain.Orders.Enums;

namespace TechShelf.UnitTests.Domain.Orders;

public class OrderHistoryEntryTests
{
    [Fact]
    public void InitializesProperties_WhenValidParametersProvided()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var status = OrderStatus.Processing;
        var notes = "Order has been processed.";

        // Act
        var orderHistory = new OrderHistoryEntry(orderId, status, notes);

        // Assert
        orderHistory.Id.Should().NotBeEmpty();
        orderHistory.OrderId.Should().Be(orderId);
        orderHistory.Status.Should().Be(status);
        orderHistory.Date.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        orderHistory.Notes.Should().Be(notes);
    }

    [Fact]
    public void InitializesNotesToNull_WhenNotesAreNotProvided()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var status = OrderStatus.Processing;

        // Act
        var orderHistory = new OrderHistoryEntry(orderId, status, null);

        // Assert
        orderHistory.Notes.Should().BeNull();
    }

    [Fact]
    public void ThrowsArgumentException_WhenOrderIdIsEmpty()
    {
        // Arrange
        var orderId = Guid.Empty;
        var status = OrderStatus.Processing;
        var notes = "Order completed.";

        // Act
        Action act = () => new OrderHistoryEntry(orderId, status, notes);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("OrderId must not be empty.");
    }
}
