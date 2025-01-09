using AutoFixture;
using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using TechShelf.API.Common.Http;
using TechShelf.API.Common.Requests.Users;
using TechShelf.API.Common.Responses;
using TechShelf.API.Controllers;
using TechShelf.Application.Features.Users.Commands.Login;
using TechShelf.Application.Features.Users.Commands.RefreshToken;
using TechShelf.Application.Features.Users.Commands.RegisterCustomer;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Features.Users.Queries.GetUserInfo;
using TechShelf.Infrastructure.Identity.Options;

namespace TechShelf.UnitTests.Api.Controllers;

public class UsersControllerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly JwtOptions _jwtOptions;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _fixture = new Fixture();
        _mediatorMock = new Mock<IMediator>();
        var _jwtOptionsMock = new Mock<IOptions<JwtOptions>>();
        _jwtOptions = _fixture.Create<JwtOptions>();
        _jwtOptionsMock.Setup(o => o.Value).Returns(_jwtOptions); 
        _controller = new UsersController(_mediatorMock.Object, _jwtOptionsMock.Object);
    }

    [Fact]
    public async Task RegisterCustomer_ReturnsOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var request = _fixture.Create<RegisterCustomerRequest>();
        var token = _fixture.Create<string>();
        var refreshToken = _fixture.Create<string>();
        var expectedTokenResponse = new TokenResponse(token);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RegisterCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenDto(token, refreshToken));

        var httpContext = new DefaultHttpContext();
        var controllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
        _controller.ControllerContext = controllerContext;

        // Act
        var result = await _controller.RegisterCustomer(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedTokenResponse);

        _mediatorMock.Verify(m => m.Send(It.IsAny<RegisterCustomerCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        VerifyRefreshTokenBeingSet(httpContext);
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
        var refreshToken = _fixture.Create<string>();
        var expectedTokenResponse = new TokenResponse(token);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenDto(token, refreshToken));

        var httpContext = new DefaultHttpContext();
        var controllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
        _controller.ControllerContext = controllerContext;

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedTokenResponse);

        _mediatorMock.Verify(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        VerifyRefreshTokenBeingSet(httpContext);
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
        Func<Task> act = _controller.GetCurrentUser;

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

    [Fact]
    public async Task RefreshToken_ReturnsOk_WhenRefreshTokenSucceeds()
    {
        // Arrange
        var email = _fixture.Create<string>();
        var token = GetTestJwt(email);
        var refreshToken = _fixture.Create<string>();
        var expectedTokenResponse = new TokenResponse(token);

        var authHeader = $"Bearer {token}";

        var mockCookies = new Mock<IRequestCookieCollection>();
        mockCookies.Setup(c => c[Cookies.RefreshToken]).Returns(refreshToken);

        var httpContext = new DefaultHttpContext
        {
            Request = { Cookies = mockCookies.Object }
        };
        httpContext.Request.Headers.Authorization = authHeader;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenDto(token, refreshToken));

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedTokenResponse);

        _mediatorMock.Verify(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        VerifyRefreshTokenBeingSet(httpContext);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenRefreshTokenIsMissing()
    {
        // Act
        var httpContext = new DefaultHttpContext();
        var mockCookies = new Mock<IRequestCookieCollection>();
        httpContext.Request.Cookies = mockCookies.Object;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenEmailClaimIsMissing()
    {
        // Arrange
        var refreshToken = _fixture.Create<string>();
        var token = GetTestJwt();

        var httpContext = new DefaultHttpContext();
        var authHeader = $"Bearer {token}";
        httpContext.Request.Headers.Authorization = authHeader;

        var mockCookies = new Mock<IRequestCookieCollection>();
        mockCookies.Setup(c => c[Cookies.RefreshToken]).Returns(refreshToken);
        httpContext.Request.Cookies = mockCookies.Object;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    private string GetTestJwt(string? email = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new List<Claim>()
        {
            new("test-calim", "")
        };

        if (email != null)
        {
            claims.Add(new(ClaimTypes.Email, email));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes("".PadRight(100, 'x'))),
                SecurityAlgorithms.HmacSha256Signature)
        };

        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }

    private void VerifyRefreshTokenBeingSet(HttpContext httpContext)
    {
        var setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain(Cookies.RefreshToken);
    }
}
