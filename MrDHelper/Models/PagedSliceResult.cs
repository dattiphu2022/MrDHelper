using System.Collections.Generic;

namespace MrDHelper.Models
{
    public sealed class PagedSliceResult<T>
    {
        public PagedSliceResult(List<T> items, bool hasMore, int page, int pageSize)
        {
            Items = items;
            HasMore = hasMore;
            Page = page;
            PageSize = pageSize;
        }

        public List<T> Items { get; set; }
        public bool HasMore { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
