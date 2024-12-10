using FluentAssertions;
using TechShelf.Application.Common;

namespace TechShelf.UnitTests.Application.Common;

public class PaginationHelperTests
{
    [Theory]
    [InlineData(1, 10, 0, 10)] 
    [InlineData(2, 10, 10, 10)]
    [InlineData(3, 20, 40, 20)] 
    public void CalculatePagination_ReturnsCorrectSkipAndTake_WhenInputValid
        (int pageIndex, int pageSize, int expectedSkip, int expectedTake)
    {
        // Act
        var (skip, take) = PaginationHelper.CalculatePagination(pageIndex, pageSize);

        // Assert
        skip.Should().Be(expectedSkip);
        take.Should().Be(expectedTake);
    }

    [Theory]
    [InlineData(0, 10)] 
    [InlineData(-1, 10)]
    public void CalculatePagination_InvalidPageIndex_ThrowsArgumentOutOfRangeException(int pageIndex, int pageSize)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PaginationHelper.CalculatePagination(pageIndex, pageSize));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void CalculatePagination_InvalidPageSize_ThrowsArgumentOutOfRangeException(int pageIndex, int pageSize)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PaginationHelper.CalculatePagination(pageIndex, pageSize));
    }
}
