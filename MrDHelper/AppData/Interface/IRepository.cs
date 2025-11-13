using MrDHelper.AppData.Attributes;
using System.Linq.Expressions;

namespace MrDHelper.AppData.Interface;


public interface IRepository<T> : IDisposable
    where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);

    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken);

    [ExcludeFromCancellationTokenCheck]
    void Update(T entity);

    [ExcludeFromCancellationTokenCheck]
    void Remove(T entity);

    [ExcludeFromCancellationTokenCheck]
    void RemoveRange(IEnumerable<T> entities);
    Task<bool> AnyAsync(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
