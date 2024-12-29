using ErrorOr;

namespace TechShelf.Application.Interfaces.Auth;

public interface ITokenService
{
    Task<ErrorOr<string>> GetTokenAsync(string email);
    Task<ErrorOr<string>> GetRefreshTokenAsync(string email);
    Task<bool> ValidateRefreshTokenAsync(string email, string refreshToken);
}
