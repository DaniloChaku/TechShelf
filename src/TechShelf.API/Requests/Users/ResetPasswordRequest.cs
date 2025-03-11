namespace TechShelf.API.Requests.Users;

public record ResetPasswordRequest(
    string Token,
    string Email,
    string Password);
