using ErrorOr;

namespace TechShelf.Infrastructure.Identity.Errors;

public static class UserErrors
{
    public static Error NotFound(string email) => Error.NotFound("User.NotFound", $"User {email} was not found");
}
