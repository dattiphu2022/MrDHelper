using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
