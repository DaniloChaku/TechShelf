using Ardalis.Specification;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TechShelf.Domain.Entities;
using TechShelf.Infrastructure.Data;
using TechShelf.Infrastructure.Data.Repositories;

namespace TechShelf.IntegrationTests.Integration.Data.Repositories;

public class GenericRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GenericRepository<Product> _repository;

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

    private bool _disposed;

    public GenericRepositoryTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<GenericRepositoryTests>()
            .Build();

        var connectionString = configuration["ConnectionStrings:TestDatabase"];
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString,
            o => o.SetPostgresVersion(12, 0))
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
        SeedTestData(_dbContext);

        _repository = new GenericRepository<Product>(_dbContext);
    }

    private void SeedTestData(ApplicationDbContext dbContext)
    {
        dbContext.Brands.AddRange(_brands);
        dbContext.Categories.AddRange(_categories);
        dbContext.Products.AddRange(_testProducts);
        dbContext.SaveChanges();
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dbContext.Database.EnsureDeleted();
                _dbContext.Dispose();
            }

            _disposed = true;
        }
    }
}

