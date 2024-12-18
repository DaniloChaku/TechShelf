using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TechShelf.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;
    [StringLength(20)]
    public override string? PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }
}
