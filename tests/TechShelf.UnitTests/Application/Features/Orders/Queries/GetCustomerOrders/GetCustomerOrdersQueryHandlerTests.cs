using TechShelf.Application.Common.Pagination;
using TechShelf.Application.Features.Orders.Queries.GetCustomerOrders;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Orders;
using TechShelf.Domain.Orders.Specs;

namespace TechShelf.UnitTests.Application.Features.Orders.Queries.GetCustomerOrders;

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

    [Theory]
    [InlineData(1, 10, 25, 10)] // First page, 10 items per page, 25 total items
    [InlineData(2, 10, 25, 10)] // Second page, 10 items per page, 25 total items
    [InlineData(3, 5, 12, 2)]  // Third page, 5 items per page, 12 total items
    public async Task Handle_ReturnsCorrectPagedResult_WhenOrdersExist(int pageIndex, int pageSize, int totalCount, int ordersCount)
    {
        // Arrange
        var query = new GetCustomerOrdersQuery(_fixture.Create<string>(), pageIndex, pageSize);
        var orders = _fixture.CreateMany<Order>(ordersCount).ToList();
        var cancellationToken = CancellationToken.None;
        var (expectedSkip, expectedTake) = PaginationHelper.CalculatePagination(pageIndex, pageSize).Value!;

        _orderRepositoryMock
            .Setup(r => r.ListWithTotalCountAsync(It.IsAny<GetCustomerOrdersSpec>(), cancellationToken))
            .ReturnsAsync((orders, totalCount));

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Items.Should().HaveCount(orders.Count);
        result.Value.TotalCount.Should().Be(totalCount);
        result.Value.PageSize.Should().Be(pageSize);

        _orderRepositoryMock.Verify(
            r => r.ListWithTotalCountAsync(
                It.Is<GetCustomerOrdersSpec>(s => s.Query.Specification.Skip == expectedSkip &&
                s.Query.Specification.Take == expectedTake),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyPagedResult_WhenNoOrdersExist()
    {
        // Arrange
        var expectedPageIndex = 1;
        var expectedPageSize = 10;
        var query = new GetCustomerOrdersQuery(
            _fixture.Create<string>(),
            PageIndex: expectedPageIndex,
            PageSize: expectedPageSize);
        var orders = new List<Order>();
        var cancellationToken = CancellationToken.None;

        _orderRepositoryMock
            .Setup(r => r.ListWithTotalCountAsync(It.IsAny<GetCustomerOrdersSpec>(), cancellationToken))
            .ReturnsAsync((orders, 0));

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.PageIndex.Should().Be(expectedPageIndex);
        result.Value.PageSize.Should().Be(expectedPageSize);
    }

    [Theory]
    [InlineData(0, 10)]  // Invalid page index
    [InlineData(1, 0)]   // Invalid page size
    public async Task Handle_ReturnsError_WhenPaginationParametersAreInvalid(int pageIndex, int pageSize)
    {
        // Arrange
        var query = new GetCustomerOrdersQuery(
            _fixture.Create<string>(),
            pageIndex,
            pageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();

        _orderRepositoryMock.Verify(
            r => r.ListWithTotalCountAsync(It.IsAny<GetCustomerOrdersSpec>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}