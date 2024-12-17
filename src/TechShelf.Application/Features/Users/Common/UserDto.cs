namespace TechShelf.Application.Features.Users.Common;

public record UserDto(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber);
