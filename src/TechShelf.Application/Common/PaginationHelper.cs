namespace TechShelf.Application.Common;

public static class PaginationHelper
{
    public static (int Skip, int Take) CalculatePagination(int pageIndex, int pageSize)
    {
        if (pageIndex < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex), "Page index must be greater than or equal to 1.");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than or equal to 1.");
        }

        int skip = (pageIndex - 1) * pageSize;
        int take = pageSize;

        return (skip, take);
    }
}
