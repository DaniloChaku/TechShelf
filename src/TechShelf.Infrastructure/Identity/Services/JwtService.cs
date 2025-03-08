using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Domain.Users;
using TechShelf.Infrastructure.Identity.Options;

namespace TechShelf.Infrastructure.Identity.Services;

public class JwtService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TimeProvider _timeProvider;

    public JwtService(
        IOptions<JwtOptions> jwtOptions,
        UserManager<ApplicationUser> userManager,
        TimeProvider timeProvider)
    {
        _jwtOptions = jwtOptions.Value;
        _userManager = userManager;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<string>> GetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return UserErrors.NotFound(email);
        }

        var claims = await CreateUserClaimsAsync(user);
        var token = CreateSecurityToken(claims);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<List<Claim>> CreateUserClaimsAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.GivenName, user.FullName),
            new(JwtRegisteredClaimNames.Email, user.Email!)
        };

        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));
        claims.AddRange(roleClaims);

        return claims;
    }

    private JwtSecurityToken CreateSecurityToken(IEnumerable<Claim> claims)
    {
        var signingCredentials = CreateSigningCredentials();
        var expirationTime = _timeProvider.GetUtcNow()
            .UtcDateTime
            .AddMinutes(_jwtOptions.ExpiresInMinutes);

        return new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            expires: expirationTime,
            claims: claims,
            signingCredentials: signingCredentials);
    }

    private SigningCredentials CreateSigningCredentials()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));

        return new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);
    }

    public async Task<ErrorOr<string>> GetRefreshTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return UserErrors.NotFound(email);
        }

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = _timeProvider.GetUtcNow().UtcDateTime.AddDays(_jwtOptions.RefreshExpiresInDays);

        await _userManager.UpdateAsync(user);

        return refreshToken;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string email, string refreshToken)
    {
        var user = await _userManager.FindByEmailAsync(email);

        return user != null &&
            user.RefreshToken == refreshToken &&
            user.RefreshTokenExpiryTime > _timeProvider.GetUtcNow().UtcDateTime;
    }
}
