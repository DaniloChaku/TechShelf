using AutoFixture;
using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TechShelf.API.Common.Requests.Users;
using TechShelf.API.Common.Responses;
using TechShelf.API.Controllers;
using TechShelf.Application.Features.Users.Commands.Login;
using TechShelf.Application.Features.Users.Commands.RegisterCustomer;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Features.Users.Queries.GetUserInfo;

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
        var expectedTokenResponse = new TokenResponse(token);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RegisterCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _controller.RegisterCustomer(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedTokenResponse);

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

    [Fact]
    public async Task Login_ReturnsOk_WhenLoginSucceeds()
    {
        // Arrange
        var request = _fixture.Create<LoginRequest>();
        var token = _fixture.Create<string>();
        var expectedTokenResponse = new TokenResponse(token);
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedTokenResponse);
        _mediatorMock.Verify(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_ReturnsProblem_WhenLoginFails()
    {
        // Arrange
        var request = _fixture.Create<LoginRequest>();
        var error = _fixture.Create<Error>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(error);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var problemResult = result as ObjectResult;
        problemResult!.Value.Should().BeAssignableTo<ProblemDetails>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUser_ThrowsInvalidOperationException_WhenEmailClaimIsMissing()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        Func<Task> act = async () => await _controller.GetCurrentUser();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email claim is missing from the authenticated user");
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsOk_WhenUserIsFound()
    {
        // Arrange
        var email = _fixture.Create<string>();
        var userDto = _fixture.Create<UserDto>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Email, email)
        ]));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserInfoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(userDto);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsProblem_WhenUserIsNotFound()
    {
        // Arrange
        var email = _fixture.Create<string>();
        var error = _fixture.Create<Error>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Email, email)
        ]));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserInfoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(error);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var problemResult = result as ObjectResult;
        problemResult!.Value.Should().BeAssignableTo<ProblemDetails>();
    }
}
