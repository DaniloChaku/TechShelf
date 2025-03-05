using AutoFixture;
using FluentAssertions;
using TechShelf.Domain.Orders;
using TechShelf.Domain.Orders.Enums;
using TechShelf.Domain.Orders.Events;
using TechShelf.Domain.Orders.ValueObjects;

namespace TechShelf.UnitTests.Domain.Orders;

public class OrderTests
{
    private readonly Fixture _fixture = new();

    private (string email, string phoneNumber, string fullName, Address address, List<OrderItem> orderItems, string customerId) GetValidOrderData()
    {
        var email = "test@example.com";
        var phoneNumber = "1234567890";
        var fullName = "John Doe";
        var address = new Address("123 Main Street", null, "New York", "NY", "10001");
        var orderItems = new List<OrderItem>
        {
            new OrderItem(new ProductOrdered(1, "Product 1", "Description 1"), 1, 10.0m)
        };
        var customerId = "customer_123";
        return (email, phoneNumber, fullName, address, orderItems, customerId);
    }

    [Fact]
    public void Constructor_InitializesProperties_WhenValidParametersProvided()
    {
        // Arrange
        var (email, phoneNumber, fullName, address, orderItems, customerId) = GetValidOrderData();

        // Act
        var order = new Order(email, phoneNumber, fullName, address, orderItems, customerId);

        // Assert
        order.Email.Should().Be(email);
        order.PhoneNumber.Should().Be(phoneNumber);
        order.FullName.Should().Be(fullName);
        order.Address.Should().Be(address);
        order.OrderItems.Should().BeEquivalentTo(orderItems);
        order.Total.Should().Be(10.0m);
        order.History.Should().HaveCount(1);
        order.History[0].Status.Should().Be(OrderStatus.PaymentPending);
        order.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenEmailIsInvalid()
    {
        // Arrange
        var (_, phoneNumber, fullName, address, orderItems, customerId) = GetValidOrderData();

        // Act
        Action act = () => new Order(null!, phoneNumber, fullName, address, orderItems, customerId);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*email*");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenPhoneNumberIsInvalid()
    {
        // Arrange
        var (email, _, fullName, address, orderItems, customerId) = GetValidOrderData();

        // Act
        Action act = () => new Order(email, null!, fullName, address, orderItems, customerId);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*phoneNumber*");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFullNameIsInvalid()
    {
        // Arrange
        var (email, phoneNumber, _, address, orderItems, customerId) = GetValidOrderData();

        // Act
        Action act = () => new Order(email, phoneNumber, null!, address, orderItems, customerId);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*fullName*");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenAddressIsNull()
    {
        // Arrange
        var (email, phoneNumber, fullName, _, orderItems, customerId) = GetValidOrderData();

        // Act
        Action act = () => new Order(email, phoneNumber, fullName, null!, orderItems, customerId);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*address*");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOrderItemsIsEmpty()
    {
        // Arrange
        var (email, phoneNumber, fullName, address, orderItems, _) = GetValidOrderData();

        // Act
        Action act = () => new Order(email, phoneNumber, fullName, address, orderItems, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*customerId*");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenOrderItemsIsEmpty()
    {
        // Arrange
        var (email, phoneNumber, fullName, address, _, customerId) = GetValidOrderData();

        // Act
        Action act = () => new Order(email, phoneNumber, fullName, address, Enumerable.Empty<OrderItem>(), customerId);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Order must contain at least one item.");
    }

    [Fact]
    public void SetPaymentStatus_UpdatesHistory_WhenPaymentIsSuccessful()
    {
        // Arrange
        var (email, phoneNumber, fullName, address, orderItems, customerId) = GetValidOrderData();
        var order = new Order(email, phoneNumber, fullName, address, orderItems, customerId);
        var paymentIntentId = _fixture.Create<string>();

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
        var (email, phoneNumber, fullName, address, orderItems, customerId) = GetValidOrderData();
        var order = new Order(email, phoneNumber, fullName, address, orderItems, customerId);
        var paymentIntentId = _fixture.Create<string>();

        // Act
        order.SetPaymentStatus(false, paymentIntentId);

        // Assert
        order.PaymentIntentId.Should().Be(paymentIntentId);
        order.History.Should().HaveCount(2);
        order.History[1].Status.Should().Be(OrderStatus.PaymentFailed);
    }

    [Fact]
    public void SetPaymentStatus_ThrowsInvalidOperationException_WhenPaymentAlreadyProcessed()
    {
        // Arrange
        var (email, phoneNumber, fullName, address, orderItems, customerId) = GetValidOrderData();
        var order = new Order(email, phoneNumber, fullName, address, orderItems, customerId);
        var paymentIntentId = _fixture.Create<string>();
        order.SetPaymentStatus(true, paymentIntentId);

        // Act
        Action act = () => order.SetPaymentStatus(true, _fixture.Create<string>());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage(
            $"Cannot process payment for order {order.Id} - payment already processed (current status: {OrderStatus.PaymentSucceeded}).");
    }

    [Fact]
    public void SetPaymentStatus_ThrowsArgumentException_WhenPaymentIntentIdIsNull()
    {
        // Arrange
        var (email, phoneNumber, fullName, address, orderItems, customerId) = GetValidOrderData();
        var order = new Order(email, phoneNumber, fullName, address, orderItems, customerId);

        // Act
        Action act = () => order.SetPaymentStatus(true, null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*paymentIntentId*");
    }

    [Fact]
    public void SetPaymentStatus_RaisesOrderPaymentConfirmedDomainEvent_WhenPaymentIsSuccessful()
    {
        // Arrange
        var (email, phoneNumber, fullName, address, orderItems, customerId) = GetValidOrderData();
        var order = new Order(email, phoneNumber, fullName, address, orderItems, customerId);
        var paymentIntentId = _fixture.Create<string>();

        // Act
        order.SetPaymentStatus(true, paymentIntentId);

        // Assert
        var domainEvent = order.DomainEvents.OfType<OrderPaymentConfirmedDomainEvent>().FirstOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.OrderId.Should().Be(order.Id);
        domainEvent.CustomerEmail.Should().Be(email);
    }
}
