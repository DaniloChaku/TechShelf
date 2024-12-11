using FluentAssertions;
using Moq;
using TechShelf.Application.Features.Categories.Queries.GetAllCategories;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Entities;

namespace TechShelf.UnitTests.Application.Features.Categories.GetAllCategories;

public class GetAllCategoriesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GetAllCategoriesQueryHandler _handler;

    public GetAllCategoriesQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new GetAllCategoriesQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsCategoryDtos_WhenCategoriesExist()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics" },
            new() { Id = 2, Name = "Clothing" }
        };

        var repositoryMock = new Mock<IRepository<Category>>();
        repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(categories);

        _unitOfWorkMock.Setup(u => u.Repository<Category>()).Returns(repositoryMock.Object);

        var query = new GetAllCategoriesQuery();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(categories.Count);
        result.Should().ContainSingle(c => c.Id == 1 && c.Name == "Electronics");
        result.Should().ContainSingle(c => c.Id == 2 && c.Name == "Clothing");
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoCategoriesExist()
    {
        // Arrange
        var repositoryMock = new Mock<IRepository<Category>>();
        repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync([]);

        _unitOfWorkMock.Setup(u => u.Repository<Category>()).Returns(repositoryMock.Object);

        var query = new GetAllCategoriesQuery();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
