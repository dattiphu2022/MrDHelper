using Microsoft.EntityFrameworkCore.Query;

namespace MrDHelper.AppDomain.EfSqliteFts5
{
    public sealed class FtsSearchOptions<TEntity>
    where TEntity : class
    {
        /// <summary>
        /// Default ordering when no keyword is provided.
        /// If null, the service falls back to `OrderBy(Id)` for stable paging.
        /// </summary>
        public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? DefaultOrder { get; init; }

        /// <summary>
        /// Allows attaching `Include(...)` or other transforms to the query.
        /// </summary>
        public Func<IQueryable<TEntity>, IQueryable<TEntity>>? Include { get; init; }

        /// <summary>
        /// Filter that must be applied before paging, for example tenant ids, status, or permissions.
        /// </summary>
        public Func<IQueryable<TEntity>, IQueryable<TEntity>>? Filter { get; init; }

        /// <summary>
        /// When a filter is present, FTS scans in chunks to gather enough rows for the requested page.
        /// `ChunkSize = PageSize * ScanMultiplier`.
        /// </summary>
        public int ScanMultiplier { get; init; } = 10;

        /// <summary>
        /// Prevents unbounded scanning to avoid overly heavy queries.
        /// `MaxScan = PageSize * MaxScanPages`.
        /// </summary>
        public int MaxScanPages { get; init; } = 200; // Allow scanning up to 200 pages by default.
    }
}
