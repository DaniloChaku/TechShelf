namespace TechShelf.API.Requests.Users;

public record LoginRequest(
    string Email,
    string Password);
