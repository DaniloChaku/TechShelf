using TechShelf.Application.Features.Brands.Queries.GetAllBrands;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Products;

namespace TechShelf.UnitTests.Application.Features.Brands.GetAllBrands;

public class GetAllBrandsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GetAllBrandsQueryHandler _handler;

    public GetAllBrandsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new GetAllBrandsQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_BrandsExist_ReturnsBrandDtos()
    {
        // Arrange
        var brands = new List<Brand>
        {
            new() { Id = 1, Name = "BrandA" },
            new() { Id = 2, Name = "BrandB" }
        };

        var repositoryMock = new Mock<IRepository<Brand>>();
        repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(brands);

        _unitOfWorkMock.Setup(u => u.Repository<Brand>()).Returns(repositoryMock.Object);

        var query = new GetAllBrandsQuery();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(brands.Count);
        result.Should().ContainSingle(b => b.Id == 1 && b.Name == "BrandA");
        result.Should().ContainSingle(b => b.Id == 2 && b.Name == "BrandB");
    }

    [Fact]
    public async Task Handle_NoBrandsExist_ReturnsEmptyList()
    {
        // Arrange
        var repositoryMock = new Mock<IRepository<Brand>>();
        repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync([]);

        _unitOfWorkMock.Setup(u => u.Repository<Brand>()).Returns(repositoryMock.Object);

        var query = new GetAllBrandsQuery();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
