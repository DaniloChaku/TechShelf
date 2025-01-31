using Microsoft.AspNetCore.Identity;
using TechShelf.Infrastructure.Identity;

namespace TechShelf.IntegrationTests.TestHelpers.Seed;

public static class CustomerHelper
{
    public static void Seed(UserManager<ApplicationUser> userManager)
    {
        var customer = VerifiedCustomer;
        var result = userManager.CreateAsync(customer, VerifiedCustomerPassword).Result;
        if (!result.Succeeded) throw new Exception(result.Errors.First().Description);

        userManager.AddToRoleAsync(customer, "Customer").Wait();
    }

    public static ApplicationUser VerifiedCustomer =>
        new()
        {
            Email = "verifiedCustomer@example.com",
            UserName = "verifiedCustomer@example.com",
            PhoneNumber = "+12345678901",
            FirstName = "John",
            LastName = "Doe",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true
        };
    public const string VerifiedCustomerPassword = "Customer@123";
}
