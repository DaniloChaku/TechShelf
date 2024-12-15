namespace TechShelf.Domain.Common;

public static class UserRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string AdminSupport = "AdminSupport";
    public const string Customer = "Customer";

    public static string[] GetAllRoles()
    {
        return [SuperAdmin, AdminSupport, Customer];
    }
}
