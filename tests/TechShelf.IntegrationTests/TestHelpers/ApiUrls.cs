namespace TechShelf.IntegrationTests.TestHelpers;

public static class ApiUrls
{
    public const string Brands = "api/brands";

    public const string Categories = "api/categories";

    public const string Products = "api/products";
    public const string ProductsSearch = $"{Products}/search";

    private const string Users = "api/users";
    public const string RegisterCustomer = $"{Users}/register";
    public const string Login = $"{Users}/login";
}
