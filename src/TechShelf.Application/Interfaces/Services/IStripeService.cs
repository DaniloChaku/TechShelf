using TechShelf.Application.Features.Orders.Common.Dtos;

namespace TechShelf.Application.Interfaces.Services;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(OrderDto order);
    StripePaymentResult? HandleStripeEvent(string json, string signature);
} 

public record StripePaymentResult(Guid OrderId, bool IsSuccessful, string PaymentIntentId);