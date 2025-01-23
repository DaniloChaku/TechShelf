using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using TechShelf.API.Common;
using TechShelf.API.Common.Requests.Orders;
using TechShelf.Application.Features.Orders.Commands.CreateOrder;
using TechShelf.Application.Features.Users.Queries.GetUserInfo;
using TechShelf.Application.Interfaces.Services;

namespace TechShelf.API.Controllers;

public class OrdersController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IStripeService _stripeService;

    public OrdersController(IMediator mediator, IStripeService stripeService)
    {
        _mediator = mediator;
        _stripeService = stripeService;
    }

    [HttpPost("checkout")]
    [Authorize(Policy = Policies.AllowAnonymousAndCustomer)]
    public async Task<IActionResult> Checkout(CreateOrderRequest createOrderRequest)
    {
        var createOrderCommand = new CreateOrderCommand(
            createOrderRequest.Email, 
            createOrderRequest.PhoneNumber, 
            createOrderRequest.Name, 
            createOrderRequest.ShippingAddress, 
            createOrderRequest.BasketItems);

        if (HttpContext.User.Identity?.IsAuthenticated ?? false)
        {
            var email = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

            if (email is not null)
            {
                var user = new GetUserInfoQuery(email);
                var userResponse = await _mediator.Send(user);
                if (!userResponse.IsError)
                {
                    createOrderCommand = createOrderCommand with { UserId = userResponse.Value.Id };
                }
            }
        }

        var orderResponse = await _mediator.Send(createOrderCommand);

        if (orderResponse.IsError)
        {
            return Problem(orderResponse.Errors);
        }

        var stripeUrl = await _stripeService.CreateCheckoutSessionAsync(orderResponse.Value);
        Response.Headers.Append("Location", stripeUrl);

        return StatusCode(303);
    }
}
