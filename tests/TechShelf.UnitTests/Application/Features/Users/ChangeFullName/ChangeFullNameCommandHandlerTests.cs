using AutoFixture;
using ErrorOr;
using FluentAssertions;
using Moq;
using TechShelf.Application.Features.Users.Commands.ChangeFullName;
using TechShelf.Application.Interfaces.Auth;

namespace TechShelf.UnitTests.Application.Features.Users.ChangeFullName;

public class ChangeFullNameCommandHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly ChangeFullNameCommandHandler _handler;

    public ChangeFullNameCommandHandlerTests()
    {
        _fixture = new Fixture();
        _userServiceMock = new Mock<IUserService>();
        
        _handler = new ChangeFullNameCommandHandler(_userServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenErrorOccurs()
    {
        // Arrange
        var command = _fixture.Create<ChangeFullNameCommand>();
        var error = _fixture.Create<Error>();

        _userServiceMock
            .Setup(us => us.ChangeFullName(command.UserId, command.FullName))
            .ReturnsAsync(error);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Should().BeEquivalentTo(error);
    }

    [Fact]
    public async Task Handle_ReturnTrue_WhenUpdatingSucceeds()
    {
        // Arrange
        var command = _fixture.Create<ChangeFullNameCommand>();

        _userServiceMock
            .Setup(us => us.ChangeFullName(command.UserId, command.FullName))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }
}
