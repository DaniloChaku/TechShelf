using ErrorOr;
using Mapster;
using MediatR;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Domain.Common;

namespace TechShelf.Application.Features.Users.Commands.RegisterCustomer;

public class RegisterCustomerCommandHandler
    : IRequestHandler<RegisterCustomerCommand, ErrorOr<string>>
{
    private readonly IUserService _authService;
    private readonly ITokenService _tokenService;

    public RegisterCustomerCommandHandler(IUserService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    public async Task<ErrorOr<string>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        var userDto = request.Adapt<UserDto>();

        var registerResult = await _authService.RegisterAsync(userDto, request.Password, UserRoles.Customer);

        if (registerResult.IsError)
        {
            return registerResult.Errors;
        }

        var tokenResult = await _tokenService.GetTokenAsync(userDto.Email);

        return tokenResult;
    }
}
