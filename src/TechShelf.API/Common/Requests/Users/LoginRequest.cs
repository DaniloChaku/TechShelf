namespace TechShelf.API.Common.Requests.Users;

public record LoginRequest(
    string Email,
    string Password);
