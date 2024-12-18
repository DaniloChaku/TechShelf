using AutoFixture;
using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechShelf.API.Common.Requests.Users;
using TechShelf.API.Common.Responses;
using TechShelf.API.Controllers;
using TechShelf.Application.Features.Users.Commands.RegisterCustomer;

namespace TechShelf.UnitTests.Api.Controllers;

public class UsersControllerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _fixture = new Fixture();
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsersController(_mediatorMock.Object);
    }

    [Fact]
    public async Task RegisterCustomer_ReturnsOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var request = _fixture.Create<RegisterCustomerRequest>();
        var token = _fixture.Create<string>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RegisterCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _controller.RegisterCustomer(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(new TokenResponse(token));

        _mediatorMock.Verify(m => m.Send(It.IsAny<RegisterCustomerCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterCustomer_ReturnsProblem_WhenRegistrationFails()
    {
        // Arrange
        var request = _fixture.Create<RegisterCustomerRequest>();
        var expectedPropName = _fixture.Create<string>();
        var expectedDescription = _fixture.Create<string>();
        var error = Error.Validation(expectedPropName, expectedDescription);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RegisterCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(error);

        // Act
        var result = await _controller.RegisterCustomer(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var problemResult = result as ObjectResult;
        problemResult!.Value.Should().BeOfType<ValidationProblemDetails>();
        var problemDetails = problemResult.Value as ValidationProblemDetails;
        problemDetails!.Errors.Should().HaveCount(1);

        _mediatorMock.Verify(m => m.Send(It.IsAny<RegisterCustomerCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
