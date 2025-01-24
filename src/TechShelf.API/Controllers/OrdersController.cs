using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.IdentityModel.Tokens.Jwt;
using TechShelf.API.Common;
using TechShelf.API.Common.Requests.Orders;
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

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json, Request.Headers["Stripe-Signature"], _stripeOptions.WhSecret);

            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                    await HandleCheckoutSessionCompletedAsync(stripeEvent);
                    break;
                case EventTypes.PaymentIntentPaymentFailed:
                    await HandlePaymentIntentFailedAsync(stripeEvent);
                    break;
            }

            return Ok();
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "Stripe exception: {Message}", e.Message);
            return BadRequest();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error in stripe webhook: {Message}", e.Message);

            // Indicates the event was received
            return Ok();
        }
    }

    private async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session ??
            throw new InvalidOperationException("Failed to parse session object.");
        await SetPaymentStatusAsync(session, true);
    }

    private async Task HandlePaymentIntentFailedAsync(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent ??
            throw new InvalidOperationException("Failed to parse payment intent object.");

        var session = await GetSessionByPaymentIntentIdAsync(paymentIntent.Id);
        if (session == null)
        {
            _logger.LogWarning("No session found for payment intent {PaymentIntentId}", paymentIntent.Id);
            return;
        }
        await SetPaymentStatusAsync(session, false);
    }

    private async Task SetPaymentStatusAsync(Session session, bool isPaymentSuccessful)
    {
        var orderId = new Guid(session.Metadata[StripeConstants.OrderIdMetadataKey]);
        var command = new SetPaymentStatusCommand(orderId, isPaymentSuccessful, session.PaymentIntentId);
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

    private async Task<Session?> GetSessionByPaymentIntentIdAsync(string paymentIntentId)
    {
        var sessionService = new SessionService();
        var sessions = await sessionService.ListAsync(new SessionListOptions()
        {
            PaymentIntent = paymentIntentId
        });

        return sessions.FirstOrDefault();
    }
}
