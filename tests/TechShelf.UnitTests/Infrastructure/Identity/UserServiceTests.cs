using AutoFixture;
using FluentAssertions;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Domain.Users;
using TechShelf.Infrastructure.Identity;
using TechShelf.Infrastructure.Identity.Services;

namespace TechShelf.UnitTests.Infrastructure.Identity;

public class UserServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<AppIdentityDbContext> _dbContextMock;
    private readonly FakeLogger<UserService> _fakeLogger;
    private readonly UserService _authService;

    public UserServiceTests()
    {
        _fixture = new Fixture();

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        var options = new DbContextOptions<AppIdentityDbContext>();
        _dbContextMock = new Mock<AppIdentityDbContext>(options);
        _fakeLogger = new FakeLogger<UserService>();

        _authService = new UserService(
            _userManagerMock.Object,
            _dbContextMock.Object,
            _fakeLogger);
    }

    #region RegisterAsync

    [Fact]
    public async Task RegisterAsync_ReturnsError_WhenUserAlreadyExists()
    {
        // Arrange
        var userDto = _fixture.Create<RegisterUserDto>();
        var password = _fixture.Create<string>();
        var role = _fixture.Create<string>();

        _userManagerMock.Setup(um => um.FindByEmailAsync(userDto.Email))
            .ReturnsAsync(_fixture.Create<ApplicationUser>());

        // Act
        var result = await _authService.RegisterAsync(userDto, password, role);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_ReturnsError_WhenUserCreationFails()
    {
        // Arrange
        var userDto = _fixture.Create<RegisterUserDto>();
        var password = _fixture.Create<string>();
        var role = _fixture.Create<string>();

        Mock<IDbContextTransaction> transactionMock = SetupTransactionMock();

        _userManagerMock.Setup(um => um.FindByEmailAsync(userDto.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        // Act
        var result = await _authService.RegisterAsync(userDto, password, role);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.RegistrationFalied);
        transactionMock.Verify(t => t.RollbackAsync(default), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsError_WhenRoleAssignmentFails()
    {
        // Arrange
        var userDto = _fixture.Create<RegisterUserDto>();
        var password = _fixture.Create<string>();
        var role = _fixture.Create<string>();

        Mock<IDbContextTransaction> transactionMock = SetupTransactionMock();

        _userManagerMock.Setup(um => um.FindByEmailAsync(userDto.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), role))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        // Act
        var result = await _authService.RegisterAsync(userDto, password, role);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.RegistrationFalied);
        transactionMock.Verify(t => t.RollbackAsync(default), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsTrue_WhenRegistrationSucceeds()
    {
        // Arrange
        var userDto = _fixture.Create<RegisterUserDto>();
        var password = _fixture.Create<string>();
        var role = _fixture.Create<string>();

        _userManagerMock.Setup(um => um.FindByEmailAsync(userDto.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), role))
            .ReturnsAsync(IdentityResult.Success);

        var transactionMock = SetupTransactionMock();

        // Act
        var result = await _authService.RegisterAsync(userDto, password, role);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
        transactionMock.Verify(t => t.CommitAsync(default), Times.Once);
    }

    private Mock<IDbContextTransaction> SetupTransactionMock()
    {
        var transactionMock = new Mock<IDbContextTransaction>();
        var databaseFacadeMock = new Mock<DatabaseFacade>(_dbContextMock.Object);

        databaseFacadeMock.Setup(db => db.BeginTransactionAsync(default))
            .ReturnsAsync(transactionMock.Object);

        _dbContextMock.Setup(db => db.Database)
            .Returns(databaseFacadeMock.Object);
        return transactionMock;
    }

    #endregion

    #region ValidatePasswordAsync

    [Fact]
    public async Task ValidatePasswordAsync_ReturnsError_WhenUserNotFound()
    {
        // Arrange
        var email = _fixture.Create<string>();
        var password = _fixture.Create<string>();

        _userManagerMock.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _authService.ValidatePasswordAsync(email, password);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.LoginAttemptFailed);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ValidatePasswordAsync_ReturnsValidationResult_WhenUserWasFound(bool expectedIsPasswordValid)
    {
        // Arrange
        var email = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var user = _fixture.Create<ApplicationUser>();

        _userManagerMock.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, password))
            .ReturnsAsync(expectedIsPasswordValid);

        // Act
        var result = await _authService.ValidatePasswordAsync(email, password);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedIsPasswordValid);
    }

    #endregion

    #region GetUserByEmailAsync

    [Fact]
    public async Task GetUserByEmailAsync_ReturnsError_WhenUserNotFound()
    {
        // Arrange
        var email = _fixture.Create<string>();
        var expectedError = UserErrors.NotFoundByEmail(email);

        _userManagerMock.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _authService.GetUserByEmailAsync(email);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ReturnsUserDto_WhenUserIsFound()
    {
        // Arrange
        var email = _fixture.Create<string>();
        var user = _fixture.Create<ApplicationUser>();
        var userDto = user.Adapt<UserDto>();
        var roles = _fixture.CreateMany<string>().ToList();
        var expectedUserDto = userDto with { Roles = roles };

        _userManagerMock.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var result = await _authService.GetUserByEmailAsync(email);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedUserDto);
    }

    #endregion

    #region ChangeFullName

    [Fact]
    public async Task ChangeFullName_ReturnsError_WhenUserNotFound()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var newFullName = _fixture.Create<string>();
        var expectedError = UserErrors.NotFoundById(userId);

        _userManagerMock.Setup(um => um.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _authService.ChangeFullName(userId, newFullName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);
    }

    [Fact]
    public async Task ChangeFullName_ReturnsInvalidOperationException_WhenUpdateFails()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var newFullName = _fixture.Create<string>();
        var user = _fixture.Create<ApplicationUser>();
        var expectedErrors = IdentityResult.Failed(new IdentityError { Description = "Error" });

        _userManagerMock.Setup(um => um.FindByIdAsync(userId))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(expectedErrors);

        // Act
        var action = () => _authService.ChangeFullName(userId, newFullName);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to update user name. Errors: *");
    }

    [Fact]
    public async Task ChangeFullName_ReturnsTrue_WhenUpdateSucceeds()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var newFullName = _fixture.Create<string>();
        var user = _fixture.Create<ApplicationUser>();

        _userManagerMock.Setup(um => um.FindByIdAsync(userId))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.ChangeFullName(userId, newFullName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
        user.FullName.Should().Be(newFullName);
    }

    #endregion

    #region GetPasswordResetToken

    [Fact]
    public async Task GetPasswordResetToken_ReturnsToken_WhenUserFound()
    {
        // Arrange
        var email = _fixture.Create<string>();
        var user = _fixture.Create<ApplicationUser>();
        var token = _fixture.Create<string>();

        _userManagerMock.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(token);

        // Act
        var result = await _authService.GetPasswordResetToken(email);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(token);
    }

    [Fact]
    public async Task GetPasswordResetToken_ReturnsError_WhenUserNotFound()
    {
        // Arrange
        var email = _fixture.Create<string>();
        var expectedError = UserErrors.NotFoundByEmail(email);

        _userManagerMock.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _authService.GetPasswordResetToken(email);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);
    }

    #endregion
}
