namespace TechShelf.API.Requests.Users;

public record RegisterCustomerRequest(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password);
