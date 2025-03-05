using AutoFixture;
using ErrorOr;
using FluentAssertions;
using Moq;
using TechShelf.Application.Features.Users.Commands.Login;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Domain.Users;

namespace TechShelf.UnitTests.Application.Features.Users.Login;

public class LoginCommandHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _fixture = new Fixture();
        _userServiceMock = new Mock<IUserService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _handler = new LoginCommandHandler(_userServiceMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenPasswordValidationFails()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
        var validationError = Error.NotFound();

        _userServiceMock
            .Setup(us => us.ValidatePasswordAsync(command.Email, command.Password))
            .ReturnsAsync(validationError);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Should().BeEquivalentTo(validationError);
    }

    [Fact]
    public async Task Handle_ReturnsLoginAttemptFailed_WhenPasswordIsInvalid()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();

        _userServiceMock
            .Setup(us => us.ValidatePasswordAsync(command.Email, command.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.LoginAttemptFailed);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenTokenGenerationFails()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
        var tokenError = Error.Failure("Token", "Token generation failed");

        _userServiceMock
            .Setup(us => us.ValidatePasswordAsync(command.Email, command.Password))
            .ReturnsAsync(true);

        _tokenServiceMock
            .Setup(ts => ts.GetTokenAsync(command.Email))
            .ReturnsAsync(tokenError);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(tokenError);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenRefreshTokenGenerationFails()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
        var token = _fixture.Create<string>();
        var refreshTokenError = _fixture.Create<Error>();

        _userServiceMock
            .Setup(us => us.ValidatePasswordAsync(command.Email, command.Password))
            .ReturnsAsync(true);

        _tokenServiceMock
            .Setup(ts => ts.GetTokenAsync(command.Email))
            .ReturnsAsync(token);

        _tokenServiceMock
            .Setup(ts => ts.GetRefreshTokenAsync(command.Email))
            .ReturnsAsync(refreshTokenError);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(refreshTokenError);
    }

    [Fact]
    public async Task Handle_ReturnsTokenDto_WhenLoginSucceeds()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
        var token = _fixture.Create<string>();
        var refreshToken = _fixture.Create<string>();
        var expectedResponse = new TokenDto(token, refreshToken);

        _userServiceMock
            .Setup(us => us.ValidatePasswordAsync(command.Email, command.Password))
            .ReturnsAsync(true);

        _tokenServiceMock
            .Setup(ts => ts.GetTokenAsync(command.Email))
            .ReturnsAsync(token);

        _tokenServiceMock
            .Setup(ts => ts.GetRefreshTokenAsync(command.Email))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedResponse);
    }
}
