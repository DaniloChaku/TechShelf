using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TechShelf.Domain.Entities;
using TechShelf.Domain.Entities.OrderAggregate;
using TechShelf.Infrastructure.Data.Outbox;

namespace TechShelf.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
#pragma warning disable CS8618
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderHistoryEntry> OrderHistoryEntries { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
