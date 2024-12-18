namespace TechShelf.API.Common.Requests.Users;

public record RegisterCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password);
