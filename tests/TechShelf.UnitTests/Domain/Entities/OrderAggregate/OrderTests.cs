using FluentAssertions;
using TechShelf.Domain.Entities.OrderAggregate;

namespace TechShelf.UnitTests.Domain.Entities.OrderAggregate;

public class OrderTests
{
    private (string email, string phoneNumber, Address address, List<OrderItem> orderItems, string? customerId) GetValidOrderData()
    {
        var email = "test@example.com";
        var phoneNumber = "1234567890";
        var address = new Address("John Doe", "US", "123 Main Street", null, "New York", "NY", "10001");
        var orderItems = new List<OrderItem>
        {
            new OrderItem(new ProductOrdered(1, "Product 1", "Description 1"), 1, 10.0m)
        };
        var customerId = "customer_123";
        return (email, phoneNumber, address, orderItems, customerId);
    }

    [Fact]
    public void Constructor_InitializesProperties_WhenValidParametersProvided()
    {
        // Arrange
        var (email, phoneNumber, address, orderItems, customerId) = GetValidOrderData();

        // Act
        var order = new Order(email, phoneNumber, address, orderItems, customerId);

        // Assert
        order.Email.Should().Be(email);
        order.PhoneNumber.Should().Be(phoneNumber);
        order.Address.Should().Be(address);
        order.OrderItems.Should().BeEquivalentTo(orderItems);
        order.Total.Should().Be(10.0m);
        order.History.Should().HaveCount(1);
        order.History[0].Status.Should().Be(OrderStatus.PaymentPending);
        order.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenEmailIsInvalid()
    {
        // Arrange
        var (_, phoneNumber, address, orderItems, customerId) = GetValidOrderData();

        // Act
        Action act = () => new Order(null!, phoneNumber, address, orderItems, customerId);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*email*");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenPhoneNumberIsInvalid()
    {
        // Arrange
        var (email, _, address, orderItems, customerId) = GetValidOrderData();

        // Act
        Action act = () => new Order(email, null!, address, orderItems, customerId);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*phoneNumber*");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenAddressIsNull()
    {
        // Arrange
        var (email, phoneNumber, _, orderItems, customerId) = GetValidOrderData();

        // Act
        Action act = () => new Order(email, phoneNumber, null!, orderItems, customerId);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*address*");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenOrderItemsIsEmpty()
    {
        // Arrange
        var (email, phoneNumber, address, _, customerId) = GetValidOrderData();

        // Act
        Action act = () => new Order(email, phoneNumber, address, Enumerable.Empty<OrderItem>(), customerId);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Order must contain at least one item.");
    }

    [Fact]
    public void SetPaymentStatus_UpdatesHistory_WhenPaymentIsSuccessful()
    {
        // Arrange
        var (email, phoneNumber, address, orderItems, customerId) = GetValidOrderData();
        var order = new Order(email, phoneNumber, address, orderItems, customerId);
        var paymentIntentId = "pi_123456789";

        // Act
        order.SetPaymentStatus(true, paymentIntentId);

        // Assert
        order.PaymentIntentId.Should().Be(paymentIntentId);
        order.History.Should().HaveCount(2);
        order.History[1].Status.Should().Be(OrderStatus.PaymentSucceeded);
    }

    [Fact]
    public void SetPaymentStatus_UpdatesHistory_WhenPaymentFails()
    {
        // Arrange
        var (email, phoneNumber, address, orderItems, customerId) = GetValidOrderData();
        var order = new Order(email, phoneNumber, address, orderItems, customerId);

        // Act
        order.SetPaymentStatus(false);

        // Assert
        order.PaymentIntentId.Should().BeNull();
        order.History.Should().HaveCount(2);
        order.History[1].Status.Should().Be(OrderStatus.PaymentFailed);
    }

    [Fact]
    public void SetPaymentStatus_UpdatesHistory_WhenPaymentIsSuccessfulAndPreviousPaymentFailed()
    {
        // Arrange
        var (email, phoneNumber, address, orderItems, customerId) = GetValidOrderData();
        var order = new Order(email, phoneNumber, address, orderItems, customerId);
        var paymentIntentId = "pi_123456789";

        // Act
        order.SetPaymentStatus(false);
        order.SetPaymentStatus(true, paymentIntentId);

        // Assert
        order.PaymentIntentId.Should().Be(paymentIntentId);
        order.History.Should().HaveCount(3);
        order.History[2].Status.Should().Be(OrderStatus.PaymentSucceeded);
    }

    [Fact]
    public void SetPaymentStatus_ThrowsInvalidOperationException_WhenOrderIsAlreadyPaid()
    {
        // Arrange
        var (email, phoneNumber, address, orderItems, customerId) = GetValidOrderData();
        var order = new Order(email, phoneNumber, address, orderItems, customerId);
        var paymentIntentId = "pi_123456789";
        order.SetPaymentStatus(true, paymentIntentId);

        // Act
        Action act = () => order.SetPaymentStatus(true, paymentIntentId);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage($"Order {order.Id} is already paid for.");
    }

    [Fact]
    public void SetPaymentStatus_ThrowsArgumentException_WhenPaymentIntentIdIsNullForSuccessfulPayment()
    {
        // Arrange
        var (email, phoneNumber, address, orderItems, customerId) = GetValidOrderData();
        var order = new Order(email, phoneNumber, address, orderItems, customerId);

        // Act
        Action act = () => order.SetPaymentStatus(true, null);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage($"Payment intent ID cannot be null or empty for a successful payment on order {order.Id}.");
    }
}
