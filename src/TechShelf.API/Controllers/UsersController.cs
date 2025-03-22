using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TechShelf.API.Common.Http;
using TechShelf.API.Common.Responses;
using TechShelf.API.Requests.Users;
using TechShelf.Application.Features.Users.Commands.ChangeFullName;
using TechShelf.Application.Features.Users.Commands.ForgotPassword;
using TechShelf.Application.Features.Users.Commands.Login;
using TechShelf.Application.Features.Users.Commands.RefreshToken;
using TechShelf.Application.Features.Users.Commands.RegisterCustomer;
using TechShelf.Application.Features.Users.Commands.ResetPassword;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Features.Users.Queries.GetUserInfo;
using TechShelf.Infrastructure.Identity.Options;

namespace TechShelf.API.Controllers;

public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly JwtOptions _jwtOptions;

    public UsersController(IMediator mediator, IOptions<JwtOptions> jwtOptions)
    {
        _mediator = mediator;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("register")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(TokenResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(ValidationProblemDetails))]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = request.Adapt<RegisterCustomerCommand>();

        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            tokens =>
            {
                SetRefreshTokenCookie(tokens.RefreshToken);
                return Ok(new TokenResponse(tokens.Token));
            },
            errors => Problem(errors));
    }

    [HttpPost("login")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(TokenResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(ValidationProblemDetails))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = request.Adapt<LoginCommand>();

        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            tokens =>
            {
                SetRefreshTokenCookie(tokens.RefreshToken);
                return Ok(new TokenResponse(tokens.Token));
            },
            errors => Problem(errors));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(TokenResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(ValidationProblemDetails))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken = default)
    {
        var refreshToken = Request.Cookies[Cookies.RefreshToken];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized();
        }

        var email = GetEmailClaimFromJwt();
        if (email is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new RefreshTokenCommand(email, refreshToken), cancellationToken);

        return result.Match(
            tokens =>
            {
                SetRefreshTokenCookie(tokens.RefreshToken);
                return Ok(new TokenResponse(tokens.Token));
            },
            Problem);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(UserDto))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (email is null)
        {
            throw new InvalidOperationException("Email claim is missing from the authenticated user");
        }

        var query = new GetUserInfoQuery(email);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            user => Ok(user),
            errors => Problem(errors));
    }

    [Authorize]
    [HttpPut("me/name")]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(ValidationProblemDetails))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeFullName([FromBody] ChangeFullNameRequest request,
    CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            throw new InvalidOperationException("User ID claim is missing from the authenticated user");
        }

        var command = new ChangeFullNameCommand(userId, request.FullName);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => Ok(),
            Problem);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new ResetPasswordCommand(request.Token, request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => Ok(),
            Problem);
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshExpiresInDays)
        };
        Response.Cookies.Append(Cookies.RefreshToken, refreshToken, cookieOptions);
    }

    private string? GetEmailClaimFromJwt()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader)) return null;

        var token = authHeader.Substring("Bearer ".Length);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        var emailClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email);
        return emailClaim?.Value;
    }
}
