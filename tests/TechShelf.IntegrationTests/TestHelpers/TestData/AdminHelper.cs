using TechShelf.Infrastructure.Identity;
using TechShelf.Infrastructure.Identity.Options;

namespace TechShelf.IntegrationTests.TestHelpers.TestData;

public static class AdminHelper
{
    public static AdminOptions.SuperAdmin SuperAdminOptions =>
            new()
            {
                FullName = "Super Admin",
                Email = "superadmin@example.com",
                PhoneNumber = "+1234567890",
                Password = "Admin123!"
            };
    public static ApplicationUser SuperAdmin =>
        new()
        {
            FullName = SuperAdminOptions.FullName,
            Email = SuperAdminOptions.Email,
            UserName = SuperAdminOptions.Email,
            PhoneNumber = SuperAdminOptions.PhoneNumber,
        };
}
