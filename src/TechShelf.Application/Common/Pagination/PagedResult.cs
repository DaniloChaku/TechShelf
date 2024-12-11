namespace TechShelf.Application.Common.Pagination;

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int PageIndex,
    int PageSize
);
