using AutoFixture;
using ErrorOr;
using FluentAssertions;
using Moq;
using TechShelf.Application.Features.Users.Commands.RefreshToken;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Domain.Users;

namespace TechShelf.UnitTests.Application.Features.Users.RefreshToken;

public class RefreshTokenCommandHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _fixture = new Fixture();
        _tokenServiceMock = new Mock<ITokenService>();
        _handler = new RefreshTokenCommandHandler(_tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsTokenDto_WhenRefreshTokenIsValidAndSucceeds()
    {
        // Arrange
        var command = _fixture.Create<RefreshTokenCommand>();
        var token = _fixture.Create<string>();
        var refreshToken = _fixture.Create<string>();

        _tokenServiceMock.Setup(s => s.ValidateRefreshTokenAsync(command.Email, command.RefreshToken))
            .ReturnsAsync(true);

        _tokenServiceMock.Setup(s => s.GetTokenAsync(command.Email))
            .ReturnsAsync(token);

        _tokenServiceMock.Setup(s => s.GetRefreshTokenAsync(command.Email))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(new TokenDto(token, refreshToken));

        _tokenServiceMock.Verify(s => s.ValidateRefreshTokenAsync(command.Email, command.RefreshToken), Times.Once);
        _tokenServiceMock.Verify(s => s.GetTokenAsync(command.Email), Times.Once);
        _tokenServiceMock.Verify(s => s.GetRefreshTokenAsync(command.Email), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var command = _fixture.Create<RefreshTokenCommand>();

        _tokenServiceMock.Setup(s => s.ValidateRefreshTokenAsync(command.Email, command.RefreshToken))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.InvalidRefreshToken);

        _tokenServiceMock.Verify(s => s.ValidateRefreshTokenAsync(command.Email, command.RefreshToken), Times.Once);
        _tokenServiceMock.Verify(s => s.GetTokenAsync(It.IsAny<string>()), Times.Never);
        _tokenServiceMock.Verify(s => s.GetRefreshTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenTokenCreationFails()
    {
        // Arrange
        var command = _fixture.Create<RefreshTokenCommand>();
        var tokenError = _fixture.Create<Error>();

        _tokenServiceMock.Setup(s => s.ValidateRefreshTokenAsync(command.Email, command.RefreshToken))
            .ReturnsAsync(true);

        _tokenServiceMock.Setup(s => s.GetTokenAsync(command.Email))
            .ReturnsAsync(tokenError);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(tokenError);

        _tokenServiceMock.Verify(s => s.ValidateRefreshTokenAsync(command.Email, command.RefreshToken), Times.Once);
        _tokenServiceMock.Verify(s => s.GetTokenAsync(command.Email), Times.Once);
        _tokenServiceMock.Verify(s => s.GetRefreshTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenRefreshTokenCreationFails()
    {
        // Arrange
        var command = _fixture.Create<RefreshTokenCommand>();
        var token = _fixture.Create<string>();
        var refreshTokenError = Error.Failure();

        _tokenServiceMock.Setup(s => s.ValidateRefreshTokenAsync(command.Email, command.RefreshToken))
            .ReturnsAsync(true);

        _tokenServiceMock.Setup(s => s.GetTokenAsync(command.Email))
            .ReturnsAsync(token);

        _tokenServiceMock.Setup(s => s.GetRefreshTokenAsync(command.Email))
            .ReturnsAsync(refreshTokenError);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(refreshTokenError);

        _tokenServiceMock.Verify(s => s.ValidateRefreshTokenAsync(command.Email, command.RefreshToken), Times.Once);
        _tokenServiceMock.Verify(s => s.GetTokenAsync(command.Email), Times.Once);
        _tokenServiceMock.Verify(s => s.GetRefreshTokenAsync(command.Email), Times.Once);
    }
}

