using Microsoft.EntityFrameworkCore;
using MrDHelper.Models;

namespace MrDHelper.AppData.Extensions;

public static class QueryablePagingExtensions
{
    /// <summary>
    /// Phân trang server-side, chỉ lấy (PageSize + 1) bản ghi để biết còn trang sau hay không.
    /// Không tính CountAsync() toàn bộ để giảm tải.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedAsync<T>(
            this IQueryable<T> query,
            int page,
            int pageSize,
            CancellationToken ct = default)
            where T : class
    {
        if (page < 0) page = 0;
        if (pageSize <= 0) pageSize = 20;

        var total = await query.CountAsync(ct); // Total là int (không null)

        var items = total == 0
            ? []
            : await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

        return new PagedResult<T>(items, total, page, pageSize);
    }

    public static async Task<PagedSliceResult<T>> ToPagedSliceAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken ct = default)
        where T : class
    {
        if (page < 0) page = 0;
        if (pageSize <= 0) pageSize = 20;

        var list = await query
            .Skip(page * pageSize)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        var hasMore = list.Count > pageSize;
        if (hasMore) list.RemoveAt(pageSize);

        return new PagedSliceResult<T>(list, hasMore, page, pageSize);
    }

}
