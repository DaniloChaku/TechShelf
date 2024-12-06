namespace TechShelf.Domain.Common;

public abstract class Entity<T>
    where T : notnull
{
    public T Id { get; set; } = default!;
}
