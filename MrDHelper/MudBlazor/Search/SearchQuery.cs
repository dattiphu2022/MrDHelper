namespace MrDHelper.MudBlazor.Search;

public sealed class SearchQuery
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int Page { get; set; } = 0;      // 0-based
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }

    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }

    // filter linh động (f_status=..., f_unitId=..., ...)
    public Dictionary<string, string?> Filters { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public SearchQuery Normalize(int maxPageSize = 200)
    {
        if (Page < 0) Page = 0;
        if (PageSize <= 0) PageSize = 20;
        if (PageSize > maxPageSize) PageSize = maxPageSize;

        Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
        SortBy = string.IsNullOrWhiteSpace(SortBy) ? null : SortBy.Trim();

        var cleaned = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in Filters)
        {
            if (string.IsNullOrWhiteSpace(kv.Key)) continue;
            cleaned[kv.Key.Trim()] = string.IsNullOrWhiteSpace(kv.Value) ? null : kv.Value.Trim();
        }
        Filters = cleaned;

        return this;
    }

    public SearchQuery Clone()
    {
        return new SearchQuery
        {
            Id = Guid.NewGuid(),
            Page = Page,
            PageSize = PageSize,
            Search = Search,
            SortBy = SortBy,
            SortDesc = SortDesc,
            Filters = new Dictionary<string, string?>(Filters, StringComparer.OrdinalIgnoreCase)
        };
    }

    public bool EqualsValue(SearchQuery other)
    {
        if (Page != other.Page) return false;
        if (PageSize != other.PageSize) return false;
        if (!string.Equals(Search, other.Search, StringComparison.Ordinal)) return false;
        if (!string.Equals(SortBy, other.SortBy, StringComparison.Ordinal)) return false;
        if (SortDesc != other.SortDesc) return false;

        if (Filters.Count != other.Filters.Count) return false;
        foreach (var kv in Filters)
        {
            if (!other.Filters.TryGetValue(kv.Key, out var v)) return false;
            if (!string.Equals(kv.Value, v, StringComparison.Ordinal)) return false;
        }

        return true;
    }
}
