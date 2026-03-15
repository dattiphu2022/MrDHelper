using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using MrDHelper.MudBlazor.Search;

namespace MrDHelper.MudBlazor;

public abstract class QueryBasePage : BasePage
{
    [Inject] protected SearchQueryStore QueryStore { get; set; } = default!;

    protected abstract string QueryKey { get; }

    // ===== Standard query string parameters
    [Parameter, SupplyParameterFromQuery(Name = "page")] public int? Page { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "pageSize")] public int? PageSize { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "search")] public string? Search { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "sortBy")] public string? SortBy { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "sortDesc")] public bool? SortDesc { get; set; }

    protected SearchQuery CurrentQuery => QueryStore.Get(QueryKey);

    protected override void OnInitialized()
    {
        base.OnInitialized();

        QueryStore.SetActiveKey(QueryKey);
        QueryStore.Changed += OnStoreChanged;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // URL -> store
        var q = new SearchQuery
        {
            Page = Page ?? 0,
            PageSize = PageSize ?? 20,
            Search = Search,
            SortBy = SortBy,
            SortDesc = SortDesc ?? false,
            Filters = ParseFiltersFromUrl(prefix: "f_")
        }.Normalize();

        QueryStore.Set(QueryKey, q, SearchQueryChangeSource.Url);
    }

    /// <summary>
    /// Updates the store from UI or code and optionally syncs the URL.
    /// </summary>
    protected void UpdateQueryFromUi(Action<SearchQuery> mutate, bool syncUrl = true, bool replaceHistory = true)
    {
        var changed = QueryStore.Update(QueryKey, q =>
        {
            mutate(q);
            q.Normalize();
        }, SearchQueryChangeSource.Ui);

        if (changed && syncUrl)
            SyncUrlFromState(replaceHistory);
    }

    protected void SyncUrlFromState(bool replaceHistory)
    {
        var q = QueryStore.Get(QueryKey);

        // Omit default values to keep the URL compact.
        var uri = NavigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["page"] = q.Page == 0 ? null : q.Page,
            ["pageSize"] = q.PageSize == 20 ? null : q.PageSize,
            ["search"] = string.IsNullOrWhiteSpace(q.Search) ? null : q.Search,
            ["sortBy"] = string.IsNullOrWhiteSpace(q.SortBy) ? null : q.SortBy,
            ["sortDesc"] = q.SortDesc ? true : null
        });

        NavigateSafe(uri, replace: replaceHistory);
    }

    protected virtual void OnQueryChanged(SearchQueryChangedArgs args)
    {
        // Override in a page to reload server data or trigger API calls.
    }

    private void OnStoreChanged(SearchQueryChangedArgs args)
    {
        if (!string.Equals(args.Key, QueryKey, StringComparison.OrdinalIgnoreCase))
            return;

        OnQueryChanged(args);
        _ = InvokeAsync(StateHasChanged);
    }

    private Dictionary<string, string?> ParseFiltersFromUrl(string prefix)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var qs = QueryHelpers.ParseQuery(uri.Query);

        foreach (var kv in qs)
        {
            var key = kv.Key;
            if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            var filterKey = key.Substring(prefix.Length);
            if (string.IsNullOrWhiteSpace(filterKey))
                continue;

            var value = kv.Value.Count > 0 ? kv.Value[0] : null;
            result[filterKey] = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return result;
    }

    public override void Dispose()
    {
        base.Dispose();
        QueryStore.Changed -= OnStoreChanged;
    }
}
