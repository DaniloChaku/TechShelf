﻿using TechShelf.Application.Features.Orders.Common.Dtos;

namespace TechShelf.API.Common.Requests.Orders;

public record CreateOrderRequest(
    string Email,
    string PhoneNumber,
    string Name,
    AddressDto ShippingAddress,
    IEnumerable<BasketItem> BasketItems);
