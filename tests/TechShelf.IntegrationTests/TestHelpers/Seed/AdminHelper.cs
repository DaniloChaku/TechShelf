using TechShelf.Infrastructure.Identity;
using TechShelf.Infrastructure.Identity.Options;

namespace TechShelf.IntegrationTests.TestHelpers.Seed;

public static class AdminHelper
{
    public static AdminOptions.SuperAdmin SuperAdminOptions =>
            new()
            {
                FirstName = "SuperAdmin",
                LastName = "SuperAdmin",
                Email = "superadmin@example.com",
                Password = "Admin123!"
            };
    public static ApplicationUser SuperAdmin =>
        new()
        {
            FirstName = "SuperAdmin",
            LastName = "SuperAdmin",
            Email = "superadmin@example.com",
            UserName = "superadmin@example.com"
        };
}
