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
        user.Should().NotBeNull();
        (await userManager.IsInRoleAsync(user!, UserRoles.SuperAdmin)).Should().BeTrue();
    }
}
