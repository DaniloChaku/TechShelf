namespace TechShelf.Application.Features.Users.Common;

public record RegisterUserDto(
    string FullName,
    string Email,
    string PhoneNumber,
    IEnumerable<string> Roles);
