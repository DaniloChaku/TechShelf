using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechShelf.Application.Interfaces.Data;

namespace TechShelf.Infrastructure.Data.Repositories;

public class GenericRepository<T> : IRepository<T>
    where T : class
{
    protected readonly DbContext _dbContext;

    public GenericRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>().ToListAsync(cancellationToken);
    }

    public async Task<List<T>> ListAsync(ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<T> Entities, int TotalCount)> ListWithTotalCountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        var count = await ApplySpecification(specification, true).CountAsync(cancellationToken);
        var entities = await ApplySpecification(specification).ToListAsync(cancellationToken);

        return (entities, count);
    }

    public void Add(T entity)
    {
#pragma warning disable S6966 // Awaitable method should be used
        _dbContext.Set<T>().Add(entity);
#pragma warning restore S6966 // Awaitable method should be used
    }

    public void Update(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> specification, bool isCriteriaOnly = false)
    {
        return SpecificationEvaluator.Default.GetQuery(
            _dbContext.Set<T>().AsQueryable(),
            specification,
            isCriteriaOnly);
    }
}
