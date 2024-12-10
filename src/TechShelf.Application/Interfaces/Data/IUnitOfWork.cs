namespace TechShelf.Application.Interfaces.Data;

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : class;
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
