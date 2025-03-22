using TechShelf.Domain.Orders;
using TechShelf.Domain.Orders.Specs;
using TechShelf.Domain.Orders.ValueObjects;

namespace TechShelf.UnitTests.Domain.Orders.Specs;

public class GetCustomerOrdersSpecTests
{
    private readonly Fixture _fixture;
    private const int DefaultSkip = 0;
    private const int DefaultTake = 10;

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
        var spec = new GetCustomerOrdersSpec(customerId, DefaultSkip, DefaultTake);

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
        var spec = new GetCustomerOrdersSpec(customerId, DefaultSkip, DefaultTake);

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
        var spec = new GetCustomerOrdersSpec(customerId, DefaultSkip, DefaultTake);

        // Assert
        spec.IncludeExpressions.Should().HaveCount(expectedIncludesCount);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 20)]
    [InlineData(20, 5)]
    public void AppliesCorrectPaginationParameters(int skip, int take)
    {
        // Arrange
        var customerId = _fixture.Create<string>();

        // Act
        var spec = new GetCustomerOrdersSpec(customerId, skip, take);

        // Assert
        spec.Query.Specification.Skip.Should().Be(skip);
        spec.Query.Specification.Take.Should().Be(take);
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