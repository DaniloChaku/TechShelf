using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;
using TechShelf.API.Common.Requests.Orders;
using TechShelf.API.Common.Responses;
using TechShelf.Application.Common.Pagination;
using TechShelf.Application.Features.Orders.Commands.CreateOrder;
using TechShelf.Application.Features.Orders.Commands.SetPaymentStatus;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Features.Orders.Queries.GetCustomerOrders;
using TechShelf.Application.Interfaces.Services;
using TechShelf.Domain.Common;

namespace TechShelf.API.Controllers;

public class OrdersController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IStripeService _stripeService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IMediator mediator,
        IStripeService stripeService,
        ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _stripeService = stripeService;
        _logger = logger;
    }

    [HttpPost("checkout")]
    [Authorize(Roles = UserRoles.Customer)]
    public async Task<IActionResult> Checkout(CreateOrderRequest createOrderRequest, 
        CancellationToken cancellationToken = default)
    {
        CreateOrderCommand createOrderCommand;

        var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            return Unauthorized("Invalid token structure. Please re-login");
        }

        createOrderCommand = new CreateOrderCommand(
            createOrderRequest.Email,
            createOrderRequest.PhoneNumber,
            createOrderRequest.Name,
            createOrderRequest.ShippingAddress,
            createOrderRequest.ShoppingCartItems,
            userId);

        var orderResponse = await _mediator.Send(createOrderCommand, cancellationToken);

        if (orderResponse.IsError)
        {
            return Problem(orderResponse.Errors);
        }

        var stripeUrl = await _stripeService.CreateCheckoutSessionAsync(orderResponse.Value);

        return Ok(new StripeRedirectionResponse(stripeUrl));
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        string json;
        using (var reader = new StreamReader(Request.Body))
        {
            json = await reader.ReadToEndAsync();
        }
        StripePaymentResult? paymentResult;

        try
        {
            paymentResult = _stripeService.HandleStripeEvent(json, Request.Headers["Stripe-Signature"]!);
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "Stripe exception: {Message}", e.Message);
            return BadRequest();
        }

        if (paymentResult is not null)
        {
            var command = new SetPaymentStatusCommand(
                paymentResult.OrderId,
                paymentResult.IsSuccessful,
                paymentResult.PaymentIntentId);
            var response = await _mediator.Send(command);

            if (response.IsError)
            {
                _logger.LogError(
                    "Payment status update failed for order {OrderId}. Payment success: {PaymentStatus}",
                    paymentResult.OrderId,
                    paymentResult.IsSuccessful
                );

                return Problem(response.Errors);
            }
        }

        return Ok();
    }

    [Authorize(Roles = $"{UserRoles.AdminSupport},{UserRoles.SuperAdmin}")]
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(PagedResult<OrderDto>))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(statusCode: StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCustomerOrders(
        string customerId,
        [FromQuery] int pageIndex,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomerOrdersQuery(customerId, pageIndex, pageSize);
        var orders = await _mediator.Send(query, cancellationToken);

        return orders.Match(Ok, Problem);
    }

    [Authorize(Roles = UserRoles.Customer)]
    [HttpGet("myorders")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<OrderDto>))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(statusCode: StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyOrders(
            [FromQuery] int pageIndex,
            [FromQuery] int pageSize, 
            CancellationToken cancellationToken = default)
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerId))
        {
            return Forbid();
        }

        var query = new GetCustomerOrdersQuery(customerId, pageIndex, pageSize);
        var orders = await _mediator.Send(query, cancellationToken);

        return orders.Match(Ok, Problem);
    }
}
