using AutoFixture;
using FluentAssertions;
using TechShelf.Domain.Entities.OrderAggregate;
using TechShelf.Domain.Specifications.Orders;

namespace TechShelf.UnitTests.Domain.Specifications;

public class GetCustomerOrdersSpecTests
{
    private readonly Fixture _fixture;

    public GetCustomerOrdersSpecTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void MatchesOrdersByCustomerId()
    {
        // Arrange
        var customerId = _fixture.Create<string>();
        var order = CreateOrderWithCustomerId(customerId);

        var spec = new GetCustomerOrdersSpec(customerId);

        // Act
        var result = spec.IsSatisfiedBy(order);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DoesNotMatchOrdersByDifferentCustomerId()
    {
        // Arrange
        var customerId = _fixture.Create<string>();
        var order = CreateOrderWithCustomerId(_fixture.Create<string>());

        var spec = new GetCustomerOrdersSpec(customerId);

        // Act
        var result = spec.IsSatisfiedBy(order);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasExpectedIncludesCount()
    {
        // Arrange
        var expectedIncludesCount = 4;
        var customerId = _fixture.Create<string>();

        // Act
        var spec = new GetCustomerOrdersSpec(customerId);

        // Assert
        spec.IncludeExpressions.Should().HaveCount(expectedIncludesCount);
    }

    private Order CreateOrderWithCustomerId(string customerId)
    {
        var address = _fixture.Create<Address>();
        var orderItems = _fixture.CreateMany<OrderItem>().ToList();
        return new Order(
            _fixture.Create<string>(), 
            _fixture.Create<string>(), 
            _fixture.Create<string>(),
            address,
            orderItems,
            customerId
        );
    }
}
