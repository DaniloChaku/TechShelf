using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using TechShelf.API.Controllers;
using TechShelf.Application.Features.Categories.Queries.GetAllCategories;
using TechShelf.Application.Features.Categories.Queries.Shared;

namespace TechShelf.UnitTests.Api.Controllers;

public class CategoriesControllerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _fixture = new Fixture();
        _mediatorMock = new Mock<IMediator>();
        _controller = new CategoriesController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsListOfCategoryDtos_WhenCategoriesExist()
    {
        // Arrange
        var expectedCategories = _fixture.CreateMany<CategoryDto>(5).ToList();
        var cancellationToken = new CancellationToken();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAllCategoriesQuery>(), cancellationToken))
            .ReturnsAsync(expectedCategories);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Value.Should().BeEquivalentTo(expectedCategories);

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAllCategoriesQuery>(), cancellationToken), Times.Once);
    }
}
