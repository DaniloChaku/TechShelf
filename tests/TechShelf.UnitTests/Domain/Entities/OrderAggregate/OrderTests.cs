using FluentAssertions;
using TechShelf.Domain.Entities.OrderAggregate;

namespace TechShelf.UnitTests.Domain.Entities.OrderAggregate;

public class OrderTests
{
    [Fact]
    public void Constructor_InitializesProperties_WhenValidParametersProvided()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "1234567890";
        var address = new Address("John Doe", "US", "123 Main Street", null, "New York", "NY", "10001");
        var orderItems = new List<OrderItem>
        {
            new(new ProductOrdered(1, "Product 1", "Description 1"), 1, 10.0m)
        };

        // Act
        var order = new Order(email, phoneNumber, address, orderItems);

        // Assert
        order.Email.Should().Be(email);
        order.PhoneNumber.Should().Be(phoneNumber);
        order.Address.Should().Be(address);
        order.OrderItems.Should().BeEquivalentTo(orderItems);
        order.Total.Should().Be(10.0m);
        order.History.Should().HaveCount(1);
        order.History[0].Status.Should().Be(OrderStatus.PaymentPending);
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenEmailIsInvalid()
    {
        // Arrange
        var phoneNumber = "1234567890";
        var address = new Address("John Doe", "US", "123 Main Street", null, "New York", "NY", "10001");
        var orderItems = new List<OrderItem>
        {
            new(new ProductOrdered(1, "Product 1", "Description 1"), 1, 10.0m)
        };

        // Act
        Action act = () => new Order(null!, phoneNumber, address, orderItems);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*email*");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenPhoneNumberIsInvalid()
    {
        // Arrange
        var email = "test@example.com";
        var address = new Address("John Doe", "US", "123 Main Street", null, "New York", "NY", "10001");
        var orderItems = new List<OrderItem>
        {
            new OrderItem(new ProductOrdered(1, "Product 1", "Description 1"), 1, 10.0m)
        };

        // Act
        Action act = () => new Order(email, null!, address, orderItems);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*phoneNumber*");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenAddressIsNull()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "1234567890";
        var orderItems = new List<OrderItem>
        {
            new OrderItem(new ProductOrdered(1, "Product 1", "Description 1"), 1, 10.0m)
        };

        // Act
        Action act = () => new Order(email, phoneNumber, null!, orderItems);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*address*");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenOrderItemsIsEmpty()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "1234567890";
        var address = new Address("John Doe", "US", "123 Main Street", null, "New York", "NY", "10001");

        // Act
        Action act = () => new Order(email, phoneNumber, address, Enumerable.Empty<OrderItem>());

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Order must contain at least one item.");
    }

    [Fact]
    public void SetStripePaymentIntentId_SetsStripePaymentIntentId_WhenValidIdProvided()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "1234567890";
        var address = new Address("John Doe", "US", "123 Main Street", null, "New York", "NY", "10001");
        var orderItems = new List<OrderItem>
        {
            new OrderItem(new ProductOrdered(1, "Product 1", "Description 1"), 1, 10.0m)
        };
        var order = new Order(email, phoneNumber, address, orderItems);
        var paymentIntentId = "pi_1234567890";

        // Act
        order.SetStripePaymentIntentId(paymentIntentId);

        // Assert
        order.PaymentIntentId.Should().Be(paymentIntentId);
    }

    [Fact]
    public void SetStripePaymentIntentId_ThrowsArgumentException_WhenStripePaymentIntentIdIsInvalid()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "1234567890";
        var address = new Address("John Doe", "US", "123 Main Street", null, "New York", "NY", "10001");
        var orderItems = new List<OrderItem>
        {
            new(new ProductOrdered(1, "Product 1", "Description 1"), 1, 10.0m)
        };
        var order = new Order(email, phoneNumber, address, orderItems);

        // Act
        Action act = () => order.SetStripePaymentIntentId(null!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*stripePaymentIntentId*");
    }

    [Fact]
    public void AddHistoryEntry_AddsHistoryEntry_WhenValidEntryProvided()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "1234567890";
        var address = new Address("John Doe", "US", "123 Main Street", null, "New York", "NY", "10001");
        var orderItems = new List<OrderItem>
        {
            new OrderItem(new ProductOrdered(1, "Product 1", "Description 1"), 1, 10.0m)
        };
        var order = new Order(email, phoneNumber, address, orderItems);
        var historyEntry = new OrderHistoryEntry(order.Id, OrderStatus.Processing);

        // Act
        order.AddHistoryEntry(historyEntry);

        // Assert
        order.History.Should().Contain(historyEntry);
    }

    [Fact]
    public void AddHistoryEntry_ThrowsAgrumentNullException_WhenNullValueProvided()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "1234567890";
        var address = new Address("John Doe", "US", "123 Main Street", null, "New York", "NY", "10001");
        var orderItems = new List<OrderItem>
        {
            new(new ProductOrdered(1, "Product 1", "Description 1"), 1, 10.0m)
        };
        var order = new Order(email, phoneNumber, address, orderItems);

        // Act
        var act = () => order.AddHistoryEntry(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*historyEntry*");
    }
}
