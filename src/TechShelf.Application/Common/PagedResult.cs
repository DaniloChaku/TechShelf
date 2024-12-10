namespace TechShelf.Application.Common;

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int PageIndex,
    int PageSize
);
