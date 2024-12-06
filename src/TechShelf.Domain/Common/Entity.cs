namespace TechShelf.Domain.Common;

public class Entity<T>
    where T : notnull
{
    public T Id { get; set; } = default!;
}
