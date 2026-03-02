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

        /// <summary>
        /// Filter bắt buộc áp trước paging (ví dụ: DonViIds, trạng thái, quyền...).
        /// </summary>
        public Func<IQueryable<TEntity>, IQueryable<TEntity>>? Filter { get; init; }

        /// <summary>
        /// Khi có Filter, FTS phải quét theo chunk để đủ dữ liệu cho page.
        /// ChunkSize = PageSize * ScanMultiplier.
        /// </summary>
        public int ScanMultiplier { get; init; } = 10;

        /// <summary>
        /// Chặn quét vô hạn (để tránh query quá nặng).
        /// MaxScan = PageSize * MaxScanPages.
        /// </summary>
        public int MaxScanPages { get; init; } = 200; // mặc định cho phép quét tối đa 200 pages
    }
}
