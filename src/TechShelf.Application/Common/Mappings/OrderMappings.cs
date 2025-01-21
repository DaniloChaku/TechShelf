using Mapster;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Domain.Entities.OrderAggregate;

namespace TechShelf.Application.Common.Mappings;

public static class OrderMappings
{
    public static void Configure()
    {
        TypeAdapterConfig<Address, AddressDto>
            .NewConfig()
            .Map(dest => dest.Line1, src => src.AddressLine1)
            .Map(dest => dest.Line2, src => src.AddressLine2)
            .Map(dest => dest.State, src => src.Region);
    }
}
