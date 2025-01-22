using TechShelf.Application.Features.Orders.Common.Dtos;

namespace TechShelf.Application.Interfaces.Services;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(OrderDto order);
}
