using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.IdentityModel.Tokens.Jwt;
using TechShelf.API.Common;
using TechShelf.API.Common.Requests.Orders;
using TechShelf.API.Common.Responses;
using TechShelf.Application.Features.Orders.Commands.CreateOrder;
using TechShelf.Application.Features.Orders.Commands.SetPaymentStatus;
using TechShelf.Application.Features.Users.Queries.GetUserInfo;
using TechShelf.Application.Interfaces.Services;
using TechShelf.Infrastructure.Options;
using TechShelf.Infrastructure.Services.Stripe;

namespace TechShelf.API.Controllers;

public class OrdersController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IStripeService _stripeService;
    private readonly ILogger<OrdersController> _logger;
    private readonly StripeOptions _stripeOptions;

    public OrdersController(
        IMediator mediator,
        IStripeService stripeService,
        IOptions<StripeOptions> stripeOptions,
        ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _stripeService = stripeService;
        _logger = logger;
        _stripeOptions = stripeOptions.Value;
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
            createOrderRequest.ShoppingCartItems);

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

        return Ok(new StripeRedirectionResponse(stripeUrl));
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json, Request.Headers["Stripe-Signature"], _stripeOptions.WhSecret);

            if (stripeEvent.Type is (EventTypes.PaymentIntentSucceeded or EventTypes.PaymentIntentPaymentFailed))
            {
                await HandlePaymentIntentEventAsync(stripeEvent);
            }

            return Ok();
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "Stripe exception: {Message}", e.Message);
            return BadRequest();
        }
    }

    private async Task HandlePaymentIntentEventAsync(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent ??
            throw new InvalidOperationException("Failed to parse payment intent object.");

        if (!paymentIntent.Metadata.TryGetValue(StripeConstants.OrderIdMetadataKey, out var orderIdValue))
        {
            _logger.LogError("No orderId found in metadata for payment intent {PaymentIntentId}", paymentIntent.Id);
            return;
        }

        var orderId = new Guid(orderIdValue);
        var isPaymentSuccessful = stripeEvent.Type == EventTypes.PaymentIntentSucceeded;

        // replace with creating an event and processing it in a background job"
        var command = new SetPaymentStatusCommand(orderId, isPaymentSuccessful, paymentIntent.Id);
        var response = await _mediator.Send(command);

        if (response.IsError)
        {
            _logger.LogError(
                "Payment status update failed for order {OrderId}. Payment success: {PaymentStatus}",
                orderId,
                isPaymentSuccessful
            );
        }
    }
}
