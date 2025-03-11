using AutoFixture;
using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Options;
using Moq;
using TechShelf.Application.Common.Options;
using TechShelf.Application.Features.Users.Commands.ForgotPassword;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Application.Interfaces.Services;

namespace TechShelf.UnitTests.Application.Features.Users.ForgotPassword;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly ForgotPasswordCommandHandler _handler;
    private readonly ClientUrlOptions _clientUrlOptions;

    public ForgotPasswordCommandHandlerTests()
    {
        _fixture = new Fixture();
        _userServiceMock = new Mock<IUserService>();
        _emailServiceMock = new Mock<IEmailService>();
        _clientUrlOptions = new ClientUrlOptions { ClientUrl = "https://example.com" };

        _handler = new ForgotPasswordCommandHandler(
            _userServiceMock.Object,
            _emailServiceMock.Object,
            Options.Create(_clientUrlOptions)
        );
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenTokenRetrievalFails()
    {
        // Arrange
        var command = _fixture.Create<ForgotPasswordCommand>();
        var error = _fixture.Create<Error>();
        _userServiceMock
            .Setup(us => us.GetPasswordResetToken(command.Email))
            .ReturnsAsync(error);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Should().BeEquivalentTo(error);
    }

    [Fact]
    public async Task Handle_SendsEmail_WhenTokenRetrievalSucceeds()
    {
        // Arrange
        var command = _fixture.Create<ForgotPasswordCommand>();
        var token = _fixture.Create<string>();
        _userServiceMock
            .Setup(us => us.GetPasswordResetToken(command.Email))
            .ReturnsAsync(token);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Unit.Value);
        _emailServiceMock.Verify(es => es.SendPlainTextEmailAsync(
            command.Email,
            "Reset Your Password",
            It.Is<string>(msg => msg.Contains(_clientUrlOptions.ClientUrl)),
            CancellationToken.None
        ), Times.Once);
    }
}
