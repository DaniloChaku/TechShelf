using AutoFixture;
using ErrorOr;
using FluentAssertions;
using Moq;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Features.Users.Queries.GetUserInfo;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Domain.Errors;

namespace TechShelf.UnitTests.Application.Features.Users.GetUserInfo;

public class GetUserInfoQueryHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly GetUserInfoQueryHandler _handler;

    public GetUserInfoQueryHandlerTests()
    {
        _fixture = new Fixture();
        _userServiceMock = new Mock<IUserService>();
        _handler = new GetUserInfoQueryHandler(_userServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenFaliedToGetUserInfo()
    {
        // Arrange
        var query = _fixture.Create<GetUserInfoQuery>();
        var error = _fixture.Create<Error>();

        _userServiceMock
            .Setup(us => us.GetUserByEmailAsync(query.Email))
            .ReturnsAsync(error);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(error);
    }

    [Fact]
    public async Task Handle_ReturnsUserDto_WhenUserFound()
    {
        // Arrange
        var query = _fixture.Create<GetUserInfoQuery>();
        var userDto = _fixture.Create<UserDto>();

        _userServiceMock
            .Setup(us => us.GetUserByEmailAsync(query.Email))
            .ReturnsAsync(userDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(userDto);
    }
}
