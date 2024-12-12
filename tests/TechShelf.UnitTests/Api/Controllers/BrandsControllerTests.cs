using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechShelf.API.Controllers;
using TechShelf.Application.Features.Brands.Queries.GetAllBrands;
using TechShelf.Application.Features.Brands.Queries.Shared;

namespace TechShelf.UnitTests.Api.Controllers;

public class BrandsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly BrandsController _controller;

    public BrandsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new BrandsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsListOfBrandDtos_WhenBrandsExist()
    {
        // Arrange
        var expectedBrands = new List<BrandDto>
        {
            new (1, "Brand1"),
            new (2, "Brand2")
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAllBrandsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBrands);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Value.Should().BeEquivalentTo(expectedBrands);

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAllBrandsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
