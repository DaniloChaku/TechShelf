namespace TechShelf.Application.Common;

public record PagedResult<T>(
    List<T> Result,
    int TotalCount,
    int PageIndex,
    int PageSize
);
