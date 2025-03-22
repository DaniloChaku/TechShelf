using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TechShelf.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;
    [StringLength(20)]
    public override string? PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }
    [StringLength(200)]
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
