using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechShelf.API.Common.Requests.Users;
using TechShelf.API.Common.Responses;
using TechShelf.Application.Features.Users.Commands.RegisterCustomer;

namespace TechShelf.API.Controllers;

public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request)
    {
        var command = request.Adapt<RegisterCustomerCommand>();

        var result = await _mediator.Send(command);

        return result.Match(
            token => Ok(new TokenResponse(token)),
            errors => Problem(errors));
    }
}
