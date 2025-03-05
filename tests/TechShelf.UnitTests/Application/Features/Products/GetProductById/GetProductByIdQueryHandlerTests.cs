using FluentAssertions;
using Moq;
using TechShelf.Application.Features.Products.Queries.GetProductById;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Products;
using TechShelf.Domain.Products.Specs;

namespace TechShelf.UnitTests.Application.Features.Products.GetProductById;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new GetProductByIdQueryHandler(_unitOfWorkMock.Object);
    }


    [Fact]
    public async Task Handle_ReturnsProductDto_WhenProductExists()
    {
        // Arrange
        var query = new GetProductByIdQuery(1);
        var cancellationToken = CancellationToken.None;

        var product = new Product { Id = 1, Name = "Test Product", Price = 100 };
        var repositoryMock = new Mock<IRepository<Product>>();
        repositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ProductByIdSpec>(), cancellationToken))
                      .ReturnsAsync(product);

        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(repositoryMock.Object);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(product.Id);
        result.Value.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task Handle_ProductDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        var id = 1;
        var query = new GetProductByIdQuery(id);
        var cancellationToken = CancellationToken.None;
        var expectedError = ProductErrors.NotFound(id);

        var repositoryMock = new Mock<IRepository<Product>>();
        repositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ProductByIdSpec>(), cancellationToken))
                      .ReturnsAsync(null as Product);

        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(repositoryMock.Object);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(expectedError);
    }
}
