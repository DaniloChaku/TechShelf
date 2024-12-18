using Microsoft.AspNetCore.Identity;
using Moq;
using TechShelf.Infrastructure.Identity.Services;
using TechShelf.Infrastructure.Identity;
using TechShelf.Infrastructure.Identity.Errors;
using AutoFixture;
using TechShelf.Application.Features.Users.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;

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

    [Fact]
    public async Task RegisterAsync_ReturnsError_WhenUserAlreadyExists()
    {
        // Arrange
        var userDto = _fixture.Create<UserDto>();
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
        var userDto = _fixture.Create<UserDto>();
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
        var userDto = _fixture.Create<UserDto>();
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
        var userDto = _fixture.Create<UserDto>();
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
        result.FirstError.Should().Be(UserErrors.NotFound(email));
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
}
