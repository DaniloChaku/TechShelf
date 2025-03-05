using Ardalis.Specification;

namespace TechShelf.Domain.Orders.Specs;

public class OrderByIdSpec : SingleResultSpecification<Order>
{
    public OrderByIdSpec(Guid orderId)
    {
        Query.Where(o => o.Id == orderId)
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.ProductOrdered)
            .Include(o => o.Address)
            .Include(o => o.History);
    }
}
