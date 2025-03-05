using AutoFixture;
using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechShelf.API.Common.Requests.Products;
using TechShelf.API.Controllers;
using TechShelf.Application.Common.Pagination;
using TechShelf.Application.Features.Products.Queries.GetProductById;
using TechShelf.Application.Features.Products.Queries.SearchProducts;
using TechShelf.Application.Features.Products.Queries.Shared;

namespace TechShelf.UnitTests.Api.Controllers;

public class ProductsControllerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _fixture = new Fixture();
        _mediatorMock = new Mock<IMediator>();
        _controller = new ProductsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task SearchProducts_ReturnsProducts_WhenQuerySucceeds()
    {
        // Arrange
        var request = _fixture.Create<SearchProductsRequest>();
        var expectedPagedResult = _fixture.Create<PagedResult<ProductDto>>();
        var cancellationToken = new CancellationToken();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SearchProductsQuery>(), cancellationToken))
            .ReturnsAsync(expectedPagedResult);

        // Act
        var result = await _controller.SearchProducts(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedPagedResult);

        _mediatorMock.Verify(m => m.Send(It.IsAny<SearchProductsQuery>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task SearchProducts_ReturnsProblem_WhenQueryFails()
    {
        // Arrange
        var request = _fixture.Create<SearchProductsRequest>();
        var expectedPropName = _fixture.Create<string>();
        var expectedDescription = _fixture.Create<string>();
        var error = Error.Validation(expectedPropName, expectedDescription);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SearchProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(error);

        // Act
        var result = await _controller.SearchProducts(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var problemResult = result as ObjectResult;
        problemResult!.Value.Should().BeOfType<ValidationProblemDetails>();
        var problemDetails = problemResult.Value as ValidationProblemDetails;
        problemDetails!.Errors.Should().HaveCount(1);

        _mediatorMock.Verify(m => m.Send(It.IsAny<SearchProductsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchProducts_ReturnsProduct_WhenProductExists()
    {
        // Arrange
        var expectedProduct = _fixture.Create<ProductDto>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedProduct);

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchProducts_ReturnsProblem_WhenProductNotFound()
    {
        // Arrange
        var error = Error.NotFound();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(error);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var problemResult = result as ObjectResult;
        problemResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        problemResult.Value.Should().BeOfType<ProblemDetails>();

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsProduct_WhenProductExists()
    {
        // Arrange
        var expectedProduct = _fixture.Create<ProductDto>();
        var cancellationToken = new CancellationToken();

        _mediatorMock
            .Setup(m => m.Send(
                It.Is<GetProductByIdQuery>(x => x.Id == expectedProduct.Id),
                cancellationToken))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await _controller.GetById(expectedProduct.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedProduct);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetProductByIdQuery>(x => x.Id == expectedProduct.Id),
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsProblem_WhenProductNotFound()
    {
        // Arrange
        var productId = _fixture.Create<int>();
        var error = Error.NotFound();
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetProductByIdQuery>(x => x.Id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(error);

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var problemResult = result as ObjectResult;
        problemResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        problemResult.Value.Should().BeOfType<ProblemDetails>();
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetProductByIdQuery>(x => x.Id == productId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
