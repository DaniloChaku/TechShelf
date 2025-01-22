using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Interfaces.Services;
using TechShelf.Infrastructure.Options;

namespace TechShelf.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly StripeOptions _stripeOptions;

    public StripeService(IOptions<StripeOptions> stripeOptions)
    {
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
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options);

        return session.Url;
    }
}
