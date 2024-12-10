using TechShelf.Application.Interfaces.Data;
using TechShelf.Infrastructure.Data.Repositories;

namespace TechShelf.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Dictionary<string, object> _repositories = [];

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T).Name;

        if (!_repositories.TryGetValue(type, out var repository))
        {
            var repositoryType = typeof(GenericRepository<>).MakeGenericType(typeof(T));
            repository = Activator.CreateInstance(repositoryType, _dbContext)
                ?? throw new InvalidOperationException(
                    $"Could not create repository instance for {type}");
            _repositories[type] = repository;
        }

        return (IRepository<T>)repository;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
