using Ardalis.Specification;
using TechShelf.Domain.Entities.OrderAggregate;

namespace TechShelf.Domain.Specifications.Orders;

public class GetCustomerOrdersSpec : Specification<Order>
{
    public GetCustomerOrdersSpec(string customerId)
    {
        Query.Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.ProductOrdered)
            .Include(o => o.Address)
            .Include(o => o.History);
    }
}
