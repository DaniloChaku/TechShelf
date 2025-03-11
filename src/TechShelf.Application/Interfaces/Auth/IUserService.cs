using ErrorOr;
using TechShelf.Application.Features.Users.Common;

namespace TechShelf.Application.Interfaces.Auth;

public interface IUserService
{
    Task<ErrorOr<bool>> RegisterAsync(RegisterUserDto userDto, string password, string role);
    Task<ErrorOr<bool>> ValidatePasswordAsync(string email, string password);
    Task<ErrorOr<UserDto>> GetUserByEmailAsync(string email);
    Task<ErrorOr<bool>> ChangeFullName(string userId, string newFullName);
    Task<ErrorOr<string>> GetPasswordResetToken(string email);
    Task<ErrorOr<bool>> ResetPassword(string email, string token, string newPassword);
}
