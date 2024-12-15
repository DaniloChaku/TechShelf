using TechShelf.Infrastructure.Identity;

namespace TechShelf.IntegrationTests.TestHelpers.Seed;

public static class AdminHelper
{
    public static AdminOptions.SuperAdmin SuperAdmin =>
            new()
            {
                FirstName = "SuperAdmin",
                LastName = "SuperAdmin",
                Email = "superadmin@example.com",
                Password = "Admin123!"
            };
}
