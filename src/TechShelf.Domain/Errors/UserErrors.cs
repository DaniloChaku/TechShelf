using ErrorOr;

namespace TechShelf.Domain.Errors;

public static class UserErrors
{
    public static Error NotFound(string email) => Error.NotFound("User.NotFound", $"User {email} was not found.");
    public static Error RegistrationFalied =>
        Error.Unexpected("User.Registration", "Registration failed.");
    public static Error LoginAttemptFailed =>
        Error.Unauthorized("User.Login", "Invalid email or password.");
    public static Error InvalidRefreshToken =>
        Error.Unauthorized("The refresh token is invalid or has expired. Please log in again.");
}
