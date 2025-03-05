using TechShelf.Domain.Orders;
using TechShelf.Domain.Orders.ValueObjects;
using TechShelf.Infrastructure.Data;

namespace TechShelf.IntegrationTests.TestHelpers.TestData;

public static class OrderHelper
{
    static OrderHelper()
    {
        var customer = CustomerHelper.Customer1;

        Customer1Orders = [
            new Order(
                email: customer.Email!,
                phoneNumber: customer.PhoneNumber!,
                fullName: $"{customer.FirstName} {customer.LastName}",
                address: new Address("123 Main St", null, "Anytown", "Anystate", "12345"),
                orderItems:
                [
                    new OrderItem(new ProductOrdered(1, "Product 1", "product1.jpg"), 2, 19.99m),
                    new OrderItem(new ProductOrdered(2, "Product 2", "product2.jpg"), 1, 29.99m)
                ],
                customerId: customer.Id
            ),
            new Order(
                email: customer.Email!,
                phoneNumber: customer.PhoneNumber!,
                fullName: $"{customer.FirstName} {customer.LastName}",
                address: new Address("456 Elm St", "Apt 2", "Othertown", "Otherstate", "67890"),
                orderItems:
                [
                    new OrderItem(new ProductOrdered(3, "Product 3", "product3.jpg"), 3, 9.99m),
                    new OrderItem(new ProductOrdered(4, "Product 4", "product4.jpg"), 1, 49.99m)
                ],
                customerId: customer.Id
            ),
            new Order(
                email: customer.Email!,
                phoneNumber: customer.PhoneNumber!,
                fullName: $"{customer.FirstName} {customer.LastName}",
                address: new Address("789 Oak St", null, "Sometown", "Somestate", "11223"),
                orderItems:
                [
                    new OrderItem(new ProductOrdered(5, "Product 5", "product5.jpg"), 1, 99.99m)
                ],
                customerId: customer.Id
            )];
    }

    public static void Seed(ApplicationDbContext dbContext)
    {
        dbContext.Orders.AddRange(Customer1Orders);
        dbContext.SaveChanges();
    }

    public static List<Order> Customer1Orders { get; private set; }
}
