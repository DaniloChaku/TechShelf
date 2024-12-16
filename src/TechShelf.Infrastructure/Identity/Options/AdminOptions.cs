namespace TechShelf.Infrastructure.Identity.Options;

public class AdminOptions
{
    public const string SectionName = "Admins";
    public List<SuperAdmin> SuperAdmins { get; set; } = [];

    public class SuperAdmin
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
