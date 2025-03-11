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
    public const string Me = $"{Users}/me";
    public const string RefreshToken = $"{Users}/refresh-token";
    public const string ChangeFullName = $"{Users}/me/name";
    public const string ForgotPassword = $"{Users}/forgot-password";

    public const string Orders = "api/orders";
    public const string GetCustomerOrders = $"{Orders}/customer";
    public const string GetMyOrders = $"{Orders}/myorders";
}
