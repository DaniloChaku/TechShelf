using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Interfaces.Services;
using TechShelf.Infrastructure.Options;

namespace TechShelf.Infrastructure.Services.Stripe;

public class StripeService : IStripeService
{
    private readonly StripeOptions _stripeOptions;
    private readonly ILogger<StripeService> _logger;

    public StripeService(IOptions<StripeOptions> stripeOptions, ILogger<StripeService> logger)
    {
        _logger = logger;
        _stripeOptions = stripeOptions.Value;

        StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
    }

    public async Task<string> CreateCheckoutSessionAsync(OrderDto order)
    {
        var options = new SessionCreateOptions
        {
            LineItems =
            [
                new()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(order.Total * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Order #{order.Id}",
                        },
                    },
                    Quantity = 1,
                }
            ],
            Mode = "payment",
            SuccessUrl = QueryHelpers.AddQueryString(_stripeOptions.SuccessUrl, "orderId", order.Id.ToString()),
            CancelUrl = _stripeOptions.ErrorUrl,
            Metadata = new()
            {
                { StripeConstants.OrderIdMetadataKey, order.Id.ToString() }
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new()
                {
                    { StripeConstants.OrderIdMetadataKey, order.Id.ToString() }
                }
            }
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options);

        return session.Url;
    }

    public StripePaymentResult? HandleStripeEvent(string json, string signature)
    {
        var stripeEvent = EventUtility.ConstructEvent(
                json, signature, _stripeOptions.WhSecret);

        if (stripeEvent.Type is (EventTypes.PaymentIntentSucceeded or EventTypes.PaymentIntentPaymentFailed))
        {
            return HandlePaymentIntentEventAsync(stripeEvent);
        }

        return null;
    }

    private StripePaymentResult? HandlePaymentIntentEventAsync(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent ??
            throw new InvalidOperationException("Failed to parse payment intent object.");

        if (!paymentIntent.Metadata.TryGetValue(StripeConstants.OrderIdMetadataKey, out var orderIdValue))
        {
            _logger.LogError("No orderId found in metadata for payment intent {PaymentIntentId}", paymentIntent.Id);
            return null;
        }

        var orderId = new Guid(orderIdValue);
        var isPaymentSuccessful = stripeEvent.Type == EventTypes.PaymentIntentSucceeded;

        return new(orderId, isPaymentSuccessful, paymentIntent.Id);
    }
}
