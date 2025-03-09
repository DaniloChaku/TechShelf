using ErrorOr;

namespace TechShelf.Domain.Users;

public static class UserErrors
{
    public static Error NotFoundByEmail(string email) => Error.NotFound("User.NotFoundByEmail", $"User {email} was not found.");
    public static Error NotFoundById(string id) => Error.NotFound("User.NotFoundById", $"The user with ID {id} was not found.");
    public static Error RegistrationFalied =>
        Error.Unexpected("User.Registration", "Registration failed.");
    public static Error LoginAttemptFailed =>
        Error.Unauthorized("User.Login", "Invalid email or password.");
    public static Error InvalidRefreshToken =>
        Error.Unauthorized("The refresh token is invalid or has expired. Please log in again.");
}
