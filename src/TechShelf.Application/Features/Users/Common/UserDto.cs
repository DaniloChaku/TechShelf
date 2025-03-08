namespace TechShelf.Application.Features.Users.Common;

public record UserDto(
    string Id,
    string FullName,
    string Email,
    string PhoneNumber,
    IEnumerable<string> Roles);
