using TechShelf.API.Controllers;
using TechShelf.Application.Features.Brands.Queries.GetAllBrands;
using TechShelf.Application.Features.Brands.Queries.Shared;

namespace TechShelf.UnitTests.Api.Controllers;

public class BrandsControllerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly BrandsController _controller;

    public BrandsControllerTests()
    {
        _fixture = new Fixture();
        _mediatorMock = new Mock<IMediator>();
        _controller = new BrandsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsListOfBrandDtos_WhenBrandsExist()
    {
        // Arrange
        var expectedBrands = _fixture.CreateMany<BrandDto>().ToList();
        var cancellationToken = new CancellationToken();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAllBrandsQuery>(), cancellationToken))
            .ReturnsAsync(expectedBrands);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Value.Should().BeEquivalentTo(expectedBrands);

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAllBrandsQuery>(), cancellationToken), Times.Once);
    }
}
