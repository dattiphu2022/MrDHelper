using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MrDHelper.AppData.Interface;
using MrDHelper.AppData.Attributes;

namespace MrDHelper.AppData.Base
{
    public abstract class RepositoryBase<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;

        public RepositoryBase(DbContext context)
        {
            _context = context;
        }
        
        public async Task<bool> AnyAsync(CancellationToken cancellationToken)
        {
            return await _context.Set<T>().AnyAsync(cancellationToken);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().FindAsync(id, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Set<T>().ToListAsync(cancellationToken);
        }

        public abstract Task<IEnumerable<T>> GetAllWithFullyLoadedProperties(CancellationToken cancellationToken);

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken)
        {
            await _context.Set<T>().AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
        {
            await _context.Set<T>().AddRangeAsync(entities, cancellationToken);
        }

        [ExcludeFromCancellationTokenCheck]
        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        [ExcludeFromCancellationTokenCheck]
        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        [ExcludeFromCancellationTokenCheck]
        public void RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken: cancellationToken);
        }

        //[ExcludeFromCancellationTokenCheckAttribute]
        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
