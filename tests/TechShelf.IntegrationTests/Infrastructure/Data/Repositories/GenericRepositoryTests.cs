using Ardalis.Specification;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechShelf.Domain.Products;
using TechShelf.Infrastructure.Data;
using TechShelf.Infrastructure.Data.Repositories;

namespace TechShelf.IntegrationTests.Infrastructure.Data.Repositories;

public class GenericRepositoryTests : PostgresContainerTestBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ApplicationDbContext _dbContext;
    private GenericRepository<Product> _repository;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private readonly Product[] _testProducts = [
        new Product { Id = 1, Name = "Laptop", Description = "Product 1", Stock = 1, Price = 1000, BrandId = 1, CategoryId = 1 },
        new Product { Id = 2, Name = "Smartphone", Description = "Product 2", Stock = 2, Price = 800, BrandId = 2, CategoryId = 2 },
        new Product { Id = 3, Name = "Tablet", Price = 500, Description = "Product 3", Stock = 3, BrandId = 1, CategoryId = 2 }
        ];

    private readonly Brand[] _brands = [
        new Brand { Id = 1, Name = "Tech Brand 1" },
        new Brand { Id = 2, Name = "Tech Brand 2" }
        ];

    private readonly Category[] _categories = [
        new Category { Id = 1, Name = "Electronics" },
        new Category { Id = 2, Name = "Mobile Devices" }
        ];

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // Ensure clean database
        await _dbContext.Database.EnsureCreatedAsync();

        // Seed test data
        await SeedTestDataAsync(_dbContext);

        // Initialize repository
        _repository = new GenericRepository<Product>(_dbContext);
    }

    public override async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await base.DisposeAsync();
    }


    private async Task SeedTestDataAsync(ApplicationDbContext dbContext)
    {
        dbContext.Brands.AddRange(_brands);
        dbContext.Categories.AddRange(_categories);
        dbContext.Products.AddRange(_testProducts);
        await dbContext.SaveChangesAsync();
    }

    private class ProductByIdSpecification : Specification<Product>
    {
        public ProductByIdSpecification(int id)
        {
            Query.Where(p => p.Id == id);
        }
    }

    private class ProductByMinPriceSpecification : Specification<Product>
    {
        public ProductByMinPriceSpecification(decimal minPrice)
        {
            Query.Where(p => p.Price >= minPrice);
        }
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ReturnsEntity_WhenSpecificationMatches()
    {
        // Arrange
        var specification = new ProductByIdSpecification(1);

        // Act
        var result = await _repository.FirstOrDefaultAsync(specification);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(_testProducts[0].Name);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ReturnsNull_WhenSpecificationDoesNotMatchAnyEntity()
    {
        // Arrange
        var specification = new ProductByIdSpecification(0);

        // Act
        var result = await _repository.FirstOrDefaultAsync(specification);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEntities()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(_testProducts.Length);
    }

    [Fact]
    public async Task ListAsync_ReturnsMatchingEntities()
    {
        // Arrange
        var spec = new ProductByMinPriceSpecification(800m);

        // Act
        var result = await _repository.ListAsync(spec);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListWithTotalCountAsync_ReturnsMatchingEntities()
    {
        // Arrange
        var spec = new ProductByMinPriceSpecification(800m);

        // Act
        var (result, count) = await _repository.ListWithTotalCountAsync(spec);

        // Assert
        count.Should().Be(2);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Add_AddsEntitySuccessfully()
    {
        // Arrange
        var newProduct = new Product
        {
            Id = 4,
            Name = "New Laptop",
            Description = "Product 4",
            Stock = 5,
            Price = 1200,
            BrandId = 1,
            CategoryId = 1
        };

        // Act
        _repository.Add(newProduct);
        await _dbContext.SaveChangesAsync();

        // Assert
        var addedProduct = await _dbContext.Products.FindAsync(4);
        addedProduct.Should().NotBeNull();
        addedProduct!.Name.Should().Be("New Laptop");
    }

    [Fact]
    public async Task Update_UpdatesEntitySuccessfully()
    {
        // Arrange
        var productToUpdate = await _dbContext.Products.FindAsync(1);
        productToUpdate!.Price = 1100;

        // Act
        _repository.Update(productToUpdate);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedProduct = await _dbContext.Products.FindAsync(1);
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Price.Should().Be(1100);
    }

    [Fact]
    public async Task Delete_RemovesEntitySuccessfully()
    {
        // Arrange
        var productToDelete = await _dbContext.Products.FindAsync(2);

        // Act
        _repository.Delete(productToDelete!);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedProduct = await _dbContext.Products.FindAsync(2);
        deletedProduct.Should().BeNull();
    }
}

