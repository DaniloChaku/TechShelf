using ErrorOr;
using Mapster;
using MediatR;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Entities;
using TechShelf.Domain.Entities.OrderAggregate;
using TechShelf.Domain.Errors;
using TechShelf.Domain.Specifications.Products;

namespace TechShelf.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ErrorOr<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderItemsResult = await CreateOrderItemsAsync(request.ShoppingCartItems, cancellationToken);
        if (orderItemsResult.IsError)
        {
            return orderItemsResult.Errors;
        }

        var order = CreateOrder(request, orderItemsResult.Value);

        _unitOfWork.Repository<Order>().Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Adapt<OrderDto>();
    }

    private async Task<ErrorOr<List<OrderItem>>> CreateOrderItemsAsync(
        IEnumerable<ShoppingCartItem> basketItems,
        CancellationToken cancellationToken)
    {
        var orderItems = new List<OrderItem>();

        foreach (var item in basketItems)
        {
            var spec = new ProductByIdSpec(item.ProductId);
            var product = await _unitOfWork.Repository<Product>().FirstOrDefaultAsync(spec, cancellationToken);

            if (product == null)
            {
                return OrderErrors.InvalidProductInBasket(item.ProductId);
            }

            if (product.Stock < item.Quantity)
            {
                return OrderErrors.NotEnoughStock(product.Name, product.Stock);
            }

            product.Stock -= item.Quantity;
            var productOrdered = new ProductOrdered(product.Id, product.Name, product.ThumbnailUrl);
            var orderItem = new OrderItem(productOrdered, item.Quantity, product.Price);
            orderItems.Add(orderItem);
        }

        return orderItems;
    }

    private static Order CreateOrder(CreateOrderCommand request, List<OrderItem> orderItems)
    {
        var shippingAddress = new Address(
            line1: request.ShippingAddress.Line1,
            line2: request.ShippingAddress.Line2,
            city: request.ShippingAddress.City,
            state: request.ShippingAddress.State,
            postalCode: request.ShippingAddress.PostalCode);

        return new Order(
            email: request.Email,
            phoneNumber: request.PhoneNumber,
            fullName: request.Name,
            address: shippingAddress,
            orderItems: orderItems,
            customerId: request.CustomerId);
    }
}
