using Microsoft.EntityFrameworkCore.Query;

namespace MrDHelper.AppDomain.EfSqliteFts5
{
    public sealed class FtsSearchOptions<TEntity>
    where TEntity : class
    {
        /// <summary>
        /// Thứ tự mặc định khi không search (keyword rỗng).
        /// Nếu null, service sẽ OrderBy(Id) để paging ổn định.
        /// </summary>
        public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? DefaultOrder { get; init; }

        /// <summary>
        /// Cho phép gắn Include(...) hoặc các transform khác lên query.
        /// </summary>
        public Func<IQueryable<TEntity>, IQueryable<TEntity>>? Include { get; init; }
    }
}
