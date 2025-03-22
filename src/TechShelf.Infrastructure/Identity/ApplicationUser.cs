using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using TechShelf.Domain.Users;

namespace TechShelf.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    [StringLength(UserConstants.FullNameMaxLength)]
    public string FullName { get; set; } = string.Empty;
    [StringLength(20)]
    public override string? PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }
    [StringLength(200)]
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
