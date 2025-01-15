namespace TechShelf.Application.Features.Users.Common;

public record RegisterUserDto(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    IEnumerable<string> Roles);
