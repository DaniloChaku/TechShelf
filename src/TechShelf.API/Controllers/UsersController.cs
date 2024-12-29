using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TechShelf.API.Common.Http;
using TechShelf.API.Common.Requests.Users;
using TechShelf.API.Common.Responses;
using TechShelf.Application.Features.Users.Commands.Login;
using TechShelf.Application.Features.Users.Commands.RefreshToken;
using TechShelf.Application.Features.Users.Commands.RegisterCustomer;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Features.Users.Queries.GetUserInfo;

namespace TechShelf.API.Controllers;

public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(TokenResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(ValidationProblemDetails))]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request)
    {
        var command = request.Adapt<RegisterCustomerCommand>();

        var result = await _mediator.Send(command);

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
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = request.Adapt<LoginCommand>();

        var result = await _mediator.Send(command);

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
    public async Task<IActionResult> RefreshToken()
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

        var result = await _mediator.Send(new RefreshTokenCommand(email, refreshToken));

        return result.Match(
            tokens =>
            {
                SetRefreshTokenCookie(tokens.RefreshToken);
                return Ok(new TokenResponse(tokens.Token));
            },
            errors => Problem(errors));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(UserDto))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (email is null)
        {
            throw new InvalidOperationException("Email claim is missing from the authenticated user");
        }

        var query = new GetUserInfoQuery(email);
        var result = await _mediator.Send(query);

        return result.Match(
            user => Ok(user),
            errors => Problem(errors));
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append(Cookies.RefreshToken, refreshToken, cookieOptions);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Auth()
    {
        return NoContent();
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
