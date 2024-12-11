using ErrorOr;

namespace TechShelf.Application.Common.Pagination;

public static class PaginationHelper
{
    public static ErrorOr<(int Skip, int Take)> CalculatePagination(int pageIndex, int pageSize)
    {
        if (pageIndex < 1)
        {
            return Error.Validation("PageIndex", "Page index must be greater than or equal to 1.");
        }

        if (pageSize < 1)
        {
            return Error.Validation("PageSize", "Page size must be greater than or equal to 1.");
        }

        int skip = (pageIndex - 1) * pageSize;
        int take = pageSize;

        return (skip, take);
    }
}
