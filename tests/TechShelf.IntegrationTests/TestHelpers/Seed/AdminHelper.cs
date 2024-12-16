using TechShelf.Infrastructure.Identity.Options;

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
