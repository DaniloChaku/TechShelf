using Ardalis.Specification;

namespace TechShelf.Application.Interfaces.Data;

public interface IRepository<T>
    where T : class
{
    Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<T> Entities, int TotalCount)> ListWithTotalCountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}
