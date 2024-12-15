using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TechShelf.Domain.Common;
using TechShelf.Infrastructure.Identity;
using TechShelf.IntegrationTests.TestHelpers;
using TechShelf.IntegrationTests.TestHelpers.Seed;

namespace TechShelf.IntegrationTests.Infrastructure.Identity;

public class IdentitySeederTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public IdentitySeederTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void IdentitySeeder_CreatesRoles()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Assert
        roleManager.Roles.Should().HaveCount(3);
        foreach (var role in UserRoles.GetAllRoles())
        {
            roleManager.Roles.Should().ContainSingle(r => r.Name == role);
        }
    }

    [Fact]
    public async Task IdentitySeeder_CreatesSuperAdminUser()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(AdminHelper.SuperAdmin.Email);

        // Assert
        Assert.NotNull(user);
        Assert.True(await userManager.IsInRoleAsync(user, UserRoles.SuperAdmin));
    }

    [Fact]
    public async Task IdentitySeeder_SeedsNewUser_WhenEmailIsUnique()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var newUser = new ApplicationUser
        {
            UserName = "newuser@example.com",
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            EmailConfirmed = true
        };

        // Act
        var result = await userManager.CreateAsync(newUser, "NewUser@123");
        var createdUser = await userManager.FindByEmailAsync(newUser.Email);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(createdUser);
        Assert.Equal("newuser@example.com", createdUser.Email);
    }

    [Fact]
    public async Task IdentitySeeder_DoesNotCreateDuplicateUser_WhenEmailAlreadyExists()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var existingEmail = AdminHelper.SuperAdmin.Email;

        // Act
        var newUser = new ApplicationUser
        {
            Email = existingEmail,
            FirstName = "Duplicate",
            LastName = "User"
        };

        var result = await userManager.CreateAsync(newUser, "AnotherPassword@123");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "DuplicateEmail");
    }
}
