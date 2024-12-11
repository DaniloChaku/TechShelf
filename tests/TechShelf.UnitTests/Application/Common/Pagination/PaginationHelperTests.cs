using FluentAssertions;
using TechShelf.Application.Common.Pagination;

namespace TechShelf.UnitTests.Application.Common.Pagination;

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
        var result = PaginationHelper.CalculatePagination(pageIndex, pageSize);

        // Assert
        result.IsError.Should().BeFalse();
        var (skip, take) = result.Value;
        skip.Should().Be(expectedSkip);
        take.Should().Be(expectedTake);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public void CalculatePagination_InvalidPageIndex_ReturnsError(int pageIndex, int pageSize)
    {
        // Act
        var result = PaginationHelper.CalculatePagination(pageIndex, pageSize);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.Description.Contains("page index", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void CalculatePagination_InvalidPageSize_ReturnsError(int pageIndex, int pageSize)
    {
        // Act
        var result = PaginationHelper.CalculatePagination(pageIndex, pageSize);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => 
            e.Description.Contains("page size", StringComparison.OrdinalIgnoreCase));
    }
}
