using ErrorOr;

namespace TechShelf.Application.Interfaces.Auth;

public interface ITokenService
{
    public Task<ErrorOr<string>> GetTokenAsync(string email);
}
