using Microsoft.EntityFrameworkCore;
using Moq;
using TechShelf.Infrastructure.Data;
using TechShelf.Infrastructure.Data.Repositories;

namespace TechShelf.UnitTests.Infrastructure.Data;

public class UnitOfWorkTests
{
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        _mockDbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _unitOfWork = new UnitOfWork(_mockDbContext.Object);
    }

    [Fact]
    public void Repository_CreatesNewRepositoryInstance_WhenNotAlreadyCached()
    {
        // Act
        var repository = _unitOfWork.Repository<TestEntity>();

        // Assert
        Assert.NotNull(repository);
        Assert.IsType<GenericRepository<TestEntity>>(repository);
    }

    [Fact]
    public void Repository_ReturnsCachedRepositoryInstance_WhenAlreadyCreated()
    {
        // Arrange
        var repository1 = _unitOfWork.Repository<TestEntity>();

        // Act
        var repository2 = _unitOfWork.Repository<TestEntity>();

        // Assert
        Assert.Same(repository1, repository2);
    }

    [Fact]
    public async Task SaveChangesAsync_CallsDbContextSaveChangesAsync()
    {
        // Act
        await _unitOfWork.SaveChangesAsync();

        // Assert
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class TestEntity
{
    public int Id { get; set; }
}