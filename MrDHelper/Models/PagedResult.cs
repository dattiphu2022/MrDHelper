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
        // ---- Các trường/bộ trợ giúp bổ sung (không đổi chữ ký cũ) ----
        public bool HasPreviousPage => Page > 0;

        public int PageCount => PageSize <= 0
            ? 0
            : (Total + PageSize - 1) / PageSize;

        public bool HasNextPage => Page + 1 < PageCount;

        // Tương thích với cách tính “has more” khi không muốn Count mỗi lần
        public bool HasMore => HasNextPage;

        // Chỉ số phần tử hiển thị (1-based) để show "Hiện X–Y / Tổng Z"
        public int FirstItemIndex => Total == 0 ? 0 : Page * PageSize + 1;

        public int LastItemIndex
            => Total == 0 ? 0 : System.Math.Min((Page + 1) * PageSize, Total);

        // Alias tiện lợi
        public int PageIndex => Page;
        public int Size => PageSize;
    }
}
