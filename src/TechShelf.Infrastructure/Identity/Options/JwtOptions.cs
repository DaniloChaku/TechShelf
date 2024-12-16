namespace TechShelf.Infrastructure.Identity.Options;

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; }
    public string SecretKey { get; set; } = string.Empty;
}
