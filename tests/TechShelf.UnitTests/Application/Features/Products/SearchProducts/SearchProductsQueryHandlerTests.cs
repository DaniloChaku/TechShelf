using FluentAssertions;
using Moq;
using TechShelf.Application.Features.Products.Queries.SearchProducts;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Specifications.Products;
using TechShelf.Domain.Entities;

namespace TechShelf.UnitTests.Application.Features.Products.SearchProducts;

public class SearchProductsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SearchProductsQueryHandler _handler;

    public SearchProductsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new SearchProductsQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsPagedResult_WhenRequestValid()
    {
        // Arrange
        var query = new SearchProductsQuery(
            PageIndex: 1,
            PageSize: 10,
            BrandId: 1,
            CategoryId: 1,
            Name: "Product",
            MinPrice: 0,
            MaxPrice: 100);
        var cancellationToken = CancellationToken.None;

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 50 },
            new() { Id = 2, Name = "Product 2", Price = 75 }
        };
        var expectedTotalCount = 2;

        var repositoryMock = new Mock<IRepository<Product>>();
        repositoryMock.Setup(r => r.ListWithTotalCountAsync(It.IsAny<SearchProductsPageSpec>(), cancellationToken))
                      .ReturnsAsync((products, expectedTotalCount));

        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(repositoryMock.Object);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(products.Count);
        result.TotalCount.Should().Be(expectedTotalCount);
        result.PageIndex.Should().Be(query.PageIndex);
        result.PageSize.Should().Be(query.PageSize);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyPagedResult_WhenNoProducts()
    {
        // Arrange
        var query = new SearchProductsQuery(
            PageIndex: 1,
            PageSize: 10);
        var cancellationToken = CancellationToken.None;

        var products = new List<Product>();
        var expectedTotalCount = 0;

        var repositoryMock = new Mock<IRepository<Product>>();
        repositoryMock.Setup(r => r.ListWithTotalCountAsync(It.IsAny<SearchProductsPageSpec>(), cancellationToken))
                      .ReturnsAsync((products, expectedTotalCount));

        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(repositoryMock.Object);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(expectedTotalCount);
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectSpecification()
    {
        // Arrange
        var query = new SearchProductsQuery(
            PageIndex: 2,
            PageSize: 5,
            BrandId: 1,
            CategoryId: 2,
            Name: "Test",
            MinPrice: 10,
            MaxPrice: 200);
        var cancellationToken = CancellationToken.None;

        var products = new List<Product>();
        var totalCount = 0;

        var repositoryMock = new Mock<IRepository<Product>>();
        repositoryMock.Setup(r => r.ListWithTotalCountAsync(It.IsAny<SearchProductsPageSpec>(), cancellationToken))
                      .ReturnsAsync((products, totalCount));

        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(repositoryMock.Object);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        repositoryMock.Verify(r => r.ListWithTotalCountAsync(It.Is<SearchProductsPageSpec>(spec =>
            spec.Skip == 5 && 
            spec.Take == 5 &&
            spec.WhereExpressions.Count() == 5
        ), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsArgumentOutOfRangeException_WhenPageIndexInvalid()
    {
        // Arrange
        var query = new SearchProductsQuery(
            PageIndex: -1,
            PageSize: 10);
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(query, cancellationToken);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Handle_ThrowsArgumentOutOfRangeException_WhenPageSizeInvalid()
    {
        // Arrange
        var query = new SearchProductsQuery(
            PageIndex: 1,
            PageSize: 0);
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(query, cancellationToken);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
