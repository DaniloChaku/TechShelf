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
                PhoneNumber = "+1234567890",
                Password = "Admin123!"
            };
    public static ApplicationUser SuperAdmin =>
        new()
        {
            FirstName = SuperAdminOptions.FirstName,
            LastName = SuperAdminOptions.LastName,
            Email = SuperAdminOptions.Email,
            UserName = SuperAdminOptions.Email,
            PhoneNumber= SuperAdminOptions.PhoneNumber,
        };
}
