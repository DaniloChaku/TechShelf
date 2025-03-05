using AutoFixture;
using FluentAssertions;
using TechShelf.Domain.Orders;
using TechShelf.Domain.Orders.Specs;
using TechShelf.Domain.Orders.ValueObjects;

namespace TechShelf.UnitTests.Domain.Orders.Specs;

public class OrderByIdSpecTests
{
    private readonly Fixture _fixture;

    public OrderByIdSpecTests()
    {
        _fixture = new Fixture();

        _fixture.Customize<Address>(composer => composer
            .FromFactory(() => new Address(
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>()
            )));
    }

    [Fact]
    public void MatchesOrderById()
    {
        // Arrange
        var order = _fixture.Create<Order>();

        var spec = new OrderByIdSpec(order.Id);

        // Act
        var result = spec.IsSatisfiedBy(order);

        // Assert
        result.Should().BeTrue();
        spec.IncludeExpressions.Should().HaveCount(4);
    }

    [Fact]
    public void MatchesNoProductById_WhenIdNotPresent()
    {
        // Arrange
        var order = _fixture.Create<Order>();

        var spec = new OrderByIdSpec(Guid.NewGuid());

        // Act
        var result = spec.IsSatisfiedBy(order);

        // Assert
        result.Should().BeFalse();
    }
}
