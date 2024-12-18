using ErrorOr;
using TechShelf.Application.Features.Users.Common;

namespace TechShelf.Application.Interfaces.Auth;

public interface IUserService
{
    Task<ErrorOr<bool>> RegisterAsync(UserDto userDto, string password, string role);
    Task<ErrorOr<bool>> ValidatePasswordAsync(string email, string password);
}
