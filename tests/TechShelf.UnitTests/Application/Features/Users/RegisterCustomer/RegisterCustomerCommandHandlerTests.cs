using AutoFixture;
using ErrorOr;
using FluentAssertions;
using Mapster;
using Moq;
using TechShelf.Application.Features.Users.Commands.RegisterCustomer;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Domain.Common;

namespace TechShelf.UnitTests.Application.Features.Users.RegisterCustomer;

public class RegisterCustomerCommandHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IUserService> _authServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly RegisterCustomerCommandHandler _handler;

    public RegisterCustomerCommandHandlerTests()
    {
        _fixture = new Fixture();
        _authServiceMock = new Mock<IUserService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _handler = new RegisterCustomerCommandHandler(_authServiceMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsTokenDto_WhenRegistrationAndTokenCreationSucceeds()
    {
        // Arrange
        var command = _fixture.Create<RegisterCustomerCommand>();
        var userDto = command.Adapt<RegisterUserDto>();
        var token = _fixture.Create<string>();
        var refreshToken = _fixture.Create<string>();

        _authServiceMock.Setup(s => s.RegisterAsync(userDto, command.Password, UserRoles.Customer))
            .ReturnsAsync(true);

        _tokenServiceMock.Setup(s => s.GetTokenAsync(userDto.Email))
            .ReturnsAsync(token);

        _tokenServiceMock.Setup(s => s.GetRefreshTokenAsync(userDto.Email))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(new TokenDto(token, refreshToken));

        _authServiceMock.Verify(s => s.RegisterAsync(userDto, command.Password, UserRoles.Customer), Times.Once);
        _tokenServiceMock.Verify(s => s.GetTokenAsync(userDto.Email), Times.Once);
        _tokenServiceMock.Verify(s => s.GetRefreshTokenAsync(userDto.Email), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenRegistrationFails()
    {
        // Arrange
        var command = _fixture.Create<RegisterCustomerCommand>();

        var userDto = command.Adapt<RegisterUserDto>();
        var registrationError = Error.Failure();

        _authServiceMock.Setup(s => s.RegisterAsync(userDto, command.Password, UserRoles.Customer))
            .ReturnsAsync(registrationError);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(registrationError);

        _authServiceMock.Verify(s => s.RegisterAsync(userDto, command.Password, UserRoles.Customer), Times.Once);
        _tokenServiceMock.Verify(s => s.GetTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenTokenCreationFails()
    {
        // Arrange
        var command = _fixture.Create<RegisterCustomerCommand>();

        var userDto = command.Adapt<RegisterUserDto>();
        var tokenError = Error.Failure();

        _authServiceMock.Setup(s => s.RegisterAsync(userDto, command.Password, UserRoles.Customer))
            .ReturnsAsync(true);

        _tokenServiceMock.Setup(s => s.GetTokenAsync(userDto.Email))
            .ReturnsAsync(tokenError);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(tokenError);

        _authServiceMock.Verify(s => s.RegisterAsync(userDto, command.Password, UserRoles.Customer), Times.Once);
        _tokenServiceMock.Verify(s => s.GetTokenAsync(userDto.Email), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenRefreshTokenCreationFails()
    {
        // Arrange
        var command = _fixture.Create<RegisterCustomerCommand>();

        var userDto = command.Adapt<RegisterUserDto>();
        var token = _fixture.Create<string>();
        var refreshTokenError = Error.Failure();

        _authServiceMock.Setup(s => s.RegisterAsync(userDto, command.Password, UserRoles.Customer))
            .ReturnsAsync(true);

        _tokenServiceMock.Setup(s => s.GetTokenAsync(userDto.Email))
            .ReturnsAsync(token);

        _tokenServiceMock.Setup(s => s.GetRefreshTokenAsync(userDto.Email))
            .ReturnsAsync(refreshTokenError);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(refreshTokenError);

        _authServiceMock.Verify(s => s.RegisterAsync(userDto, command.Password, UserRoles.Customer), Times.Once);
        _tokenServiceMock.Verify(s => s.GetTokenAsync(userDto.Email), Times.Once);
        _tokenServiceMock.Verify(s => s.GetRefreshTokenAsync(userDto.Email), Times.Once);
    }
}
