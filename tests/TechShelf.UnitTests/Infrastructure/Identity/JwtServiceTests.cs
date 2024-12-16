using Microsoft.AspNetCore.Identity;
using Moq;
using TechShelf.Infrastructure.Identity.Options;
using TechShelf.Infrastructure.Identity.Services;
using TechShelf.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using FluentAssertions;
using TechShelf.Infrastructure.Identity.Errors;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoFixture;
using Microsoft.Extensions.Time.Testing;

namespace TechShelf.UnitTests.Infrastructure.Identity;

public class JwtServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly JwtOptions _jwtOptions;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _fixture = new Fixture();

        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _jwtOptions = _fixture.Build<JwtOptions>()
            .With(o => o.SecretKey, "supersecretkey12345678901234567890")
            .Create();
        var jwtOptionsInstance = Options.Create(_jwtOptions);

        _fakeTimeProvider = new FakeTimeProvider();

        _jwtService = new JwtService(
            jwtOptionsInstance,
            _userManagerMock.Object,
            _fakeTimeProvider);
    }

    [Fact]
    public async Task GetTokenAsync_ReturnsError_WhenUserNotFound()
    {
        // Arrange
        var email = _fixture.Create<string>();
        _userManagerMock.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);
        var expectedError = UserErrors.NotFound(email);

        // Act
        var result = await _jwtService.GetTokenAsync(email);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(expectedError);
    }

    [Fact]
    public async Task GetTokenAsync_ReturnsToken_WhenUserExists()
    {
        // Arrange
        var expectedUser = _fixture.Create<ApplicationUser>();
        var expectedRoles = _fixture.CreateMany<string>().ToList();

        _userManagerMock.Setup(um => um.FindByEmailAsync(expectedUser.Email!))
            .ReturnsAsync(expectedUser);

        _userManagerMock.Setup(um => um.GetRolesAsync(expectedUser))
            .ReturnsAsync(expectedRoles);
        
        var expectedTokenCreationTime = DateTime.UtcNow;
        var expectedExpirationTime = expectedTokenCreationTime.AddMinutes(_jwtOptions.ExpiresInMinutes);
        _fakeTimeProvider.SetUtcNow(expectedTokenCreationTime);

        // Act
        var result = await _jwtService.GetTokenAsync(expectedUser.Email!);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNullOrEmpty();

        // Verify Token Claims
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Value);

        token.Issuer.Should().Be(_jwtOptions.Issuer);
        token.Audiences.Should().Contain(_jwtOptions.Audience);
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == expectedUser.Id);
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == expectedUser.Email);
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.GivenName && c.Value == expectedUser.FirstName);
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.FamilyName && c.Value == expectedUser.LastName);
        token.ValidTo.Should().BeCloseTo(expectedExpirationTime, TimeSpan.FromSeconds(1));

        foreach (var role in expectedRoles)
        {
            token.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role);
        }
    }
}
