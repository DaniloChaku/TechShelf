using TechShelf.Domain.Common;

namespace TechShelf.Domain.Entities;

public class Brand : Entity<int>
{
    public string Name { get; set; } = string.Empty;
}
