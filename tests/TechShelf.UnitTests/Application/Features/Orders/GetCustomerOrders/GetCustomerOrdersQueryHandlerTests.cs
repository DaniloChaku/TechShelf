using AutoFixture;
using FluentAssertions;
using Moq;
using TechShelf.Application.Features.Orders.Queries.GetCustomerOrders;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Entities.OrderAggregate;
using TechShelf.Domain.Specifications.Orders;

namespace TechShelf.UnitTests.Application.Features.Orders.GetCustomerOrders;

public class GetCustomerOrdersQueryHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Order>> _orderRepositoryMock;
    private readonly GetCustomerOrdersQueryHandler _handler;

    public GetCustomerOrdersQueryHandlerTests()
    {
        _fixture = new Fixture();

        _orderRepositoryMock = new Mock<IRepository<Order>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(_orderRepositoryMock.Object);

        _handler = new GetCustomerOrdersQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsOrders_WhenOrdersExist()
    {
        // Arrange
        var query = _fixture.Create<GetCustomerOrdersQuery>();
        var orders = _fixture.CreateMany<Order>().ToList();
        var cancellationToken = CancellationToken.None;
        _orderRepositoryMock.Setup(r => r.ListAsync(It.IsAny<GetCustomerOrdersSpec>(), cancellationToken))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(orders.Count);
        _orderRepositoryMock.Verify(r => r.ListAsync(It.IsAny<GetCustomerOrdersSpec>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyCollection_WhenOrdersDoNotExist()
    {
        // Arrange
        var query = _fixture.Create<GetCustomerOrdersQuery>();
        var orders = new List<Order>();
        var cancellationToken = CancellationToken.None;
        _orderRepositoryMock.Setup(r => r.ListAsync(It.IsAny<GetCustomerOrdersSpec>(), cancellationToken))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(orders.Count);
        _orderRepositoryMock.Verify(r => r.ListAsync(It.IsAny<GetCustomerOrdersSpec>(), cancellationToken), Times.Once);
    }
}
