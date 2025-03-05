using Ardalis.Specification;

namespace TechShelf.Domain.Orders.Specs;

public class GetCustomerOrdersSpec : Specification<Order>
{
    public GetCustomerOrdersSpec(string customerId, int skip, int take)
    {
        Query.Where(o => o.CustomerId == customerId)
            .Skip(skip)
            .Take(take)
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.ProductOrdered)
            .Include(o => o.Address)
            .Include(o => o.History);
    }
}
