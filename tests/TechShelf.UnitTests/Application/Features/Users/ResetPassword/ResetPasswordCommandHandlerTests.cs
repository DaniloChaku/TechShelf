using AutoFixture;
using ErrorOr;
using FluentAssertions;
using MediatR;
using Moq;
using TechShelf.Application.Features.Users.Commands.ResetPassword;
using TechShelf.Application.Interfaces.Auth;

namespace TechShelf.UnitTests.Application.Features.Users.ResetPassword;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Fixture _fixture;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _fixture = new();

        _handler = new ResetPasswordCommandHandler(_mockUserService.Object);
    }

    [Fact]
    public async Task Handle_ReturnsUnit_WhenResetSucceeds()
    {
        // Arrange
        var command = _fixture.Create<ResetPasswordCommand>();
        _mockUserService.Setup(x => x.ResetPassword(command.Token, command.Email, command.Password))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_ReturnError_WhenResetFails()
    {
        // Arrange
        var command = _fixture.Create<ResetPasswordCommand>();
        var error = _fixture.Create<Error>();
        _mockUserService.Setup(x => x.ResetPassword(command.Token, command.Email, command.Password))
            .ReturnsAsync(error);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Should().Be(error);
    }
}
