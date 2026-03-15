using System.Collections.Generic;

namespace MrDHelper.Models
{
    public sealed class PagedResult<T>
    {
        public PagedResult(IReadOnlyList<T> items, int total, int page, int pageSize)
        {
            Items = items;
            Total = total;
            Page = page;
            PageSize = pageSize;
        }

        public IReadOnlyList<T> Items { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        // ---- Additional helper fields without changing the existing signature ----
        public bool HasPreviousPage => Page > 0;

        public int PageCount => PageSize <= 0
            ? 0
            : (Total + PageSize - 1) / PageSize;

        public bool HasNextPage => Page + 1 < PageCount;

        // Compatible with "has more" style paging when you do not want to count every time.
        public bool HasMore => HasNextPage;

        // One-based display indexes for UI text such as "Showing X-Y of Z".
        public int FirstItemIndex => Total == 0 ? 0 : Page * PageSize + 1;

        public int LastItemIndex
            => Total == 0 ? 0 : System.Math.Min((Page + 1) * PageSize, Total);

        // Convenience aliases.
        public int PageIndex => Page;
        public int Size => PageSize;
    }
}
