using Microsoft.AspNetCore.Identity;
using TechShelf.Infrastructure.Identity;

namespace TechShelf.IntegrationTests.TestHelpers.TestData;

public static class CustomerHelper
{
    public static void Seed(UserManager<ApplicationUser> userManager)
    {
        var customer = Customer1;
        var result = userManager.CreateAsync(customer, Customer1Password).Result;
        if (!result.Succeeded) throw new Exception(result.Errors.First().Description);

        userManager.AddToRoleAsync(customer, "Customer").Wait();
    }

    public static ApplicationUser Customer1 { get; private set; } =
        new()
        {
            Email = "customer1@example.com",
            UserName = "customer1@example.com",
            PhoneNumber = "+12345678901",
            FullName = "John Doe",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true
        };
    public const string Customer1Password = "Customer@123";
}
