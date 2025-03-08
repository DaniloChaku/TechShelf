using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TechShelf.API.Common.Http;
using TechShelf.Infrastructure.Identity;

namespace TechShelf.IntegrationTests.TestHelpers;

public class JwtTestHelper
{
    public static string Key => "".PadLeft(100, 'x');
    public const string Issuer = "testIssuer";
    public const string Audience = "testAudience";
    public const int ExpiresInMinutes = 10;
    public const int RefreshExpiresInDays = 1;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTestHelper(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public string GenerateToken(ApplicationUser user, List<string> roles, bool includeEmailClaim = true)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.GivenName, user.FullName),
        };

        if (includeEmailClaim)
        {
            claims.Add(new(JwtRegisteredClaimNames.Email, user.Email!));
        }

        foreach (var role in roles)
        {
            claims.Add(new(ClaimTypes.Role, role));
        }

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            expires: DateTime.UtcNow.AddMinutes(ExpiresInMinutes),
            claims: claims,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshToken(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            throw new ArgumentException("User does not exist");
        }

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshExpiresInDays);

        await _userManager.UpdateAsync(user);

        return refreshToken;
    }

    public string GenerateInvalidToken()
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes("".PadLeft(100, '-'))),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "invalid-issuer",
            audience: "invalid-audience",
            expires: DateTime.UtcNow.AddMinutes(ExpiresInMinutes),
            claims: claims,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public void ValidateJwt(string jwtToken, DateTime issuedTime, ApplicationUser expectedUser, params string[] expectedRoles)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);

        token.Issuer.Should().Be(Issuer);
        token.Audiences.Should().Contain(Audience);
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub);
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == expectedUser.Email);
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.GivenName && c.Value == expectedUser.FullName);
        token.ValidTo.Should().BeCloseTo(issuedTime.AddMinutes(ExpiresInMinutes), TimeSpan.FromMinutes(1));

        foreach (var role in expectedRoles)
        {
            token.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role);
        }
    }

    public void ValidateRefreshToken(HttpResponseMessage response, DateTime issuedTime)
    {
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies.Should().ContainSingle(cookie => cookie.Contains($"{Cookies.RefreshToken}="));
        var refreshTokenCookie = cookies!.First(c => c.StartsWith($"{Cookies.RefreshToken}="));
        refreshTokenCookie.Should().Contain("httponly");
        refreshTokenCookie.Should().Contain("samesite=lax");

        // sample format: expires=Tue, 31 Dec 2024
        var expiresSegment = refreshTokenCookie
            .Split(';')
            .First(s => s.TrimStart().StartsWith("expires=", StringComparison.OrdinalIgnoreCase))
            .TrimStart();

        var expirationTime = DateTime.ParseExact(
            expiresSegment.Substring("expires=".Length),
            "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal);

        var expectedExpiration = issuedTime.AddDays(JwtTestHelper.RefreshExpiresInDays).Date;
        expirationTime.Date.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }
}
