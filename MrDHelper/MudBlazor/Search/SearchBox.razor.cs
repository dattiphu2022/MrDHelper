using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MrDHelper.MudBlazor.Search;

public partial class SearchBox : ComponentBase, IDisposable
{
    [Inject] public SearchQueryStore QueryStore { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public Guid Id { get; set; } = Guid.NewGuid();
    [Parameter] public string Placeholder { get; set; } = "Tìm kiếm...";
    [Parameter] public int DebounceInterval { get; set; } = 300;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }

    private CancellationTokenSource? _debounceCts;

    private string? _value;

    protected override void OnInitialized()
    {
        QueryStore.ActiveKeyChanged += OnActiveKeyChanged;
        QueryStore.Changed += OnQueryChanged;

        _value = QueryStore.GetActive().Search;
    }
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return Task.CompletedTask;

        var s = QueryStore.GetActive().Search;
        if (!string.Equals(_value, s, StringComparison.Ordinal))
        {
            _value = s;
            StateHasChanged();
        }

        return Task.CompletedTask;
    }

    private void OnActiveKeyChanged(string _)
    {
        _value = QueryStore.GetActive().Search;
        InvokeAsync(StateHasChanged);
    }

    private void OnQueryChanged(SearchQueryChangedArgs args)
    {
        var key = QueryStore.ActiveKey;
        if (string.IsNullOrWhiteSpace(key)) return;
        if (!string.Equals(args.Key, key, StringComparison.OrdinalIgnoreCase)) return;

        _value = args.Query.Search;
        InvokeAsync(StateHasChanged);
    }

    private Task OnValueChanged(string? value)
    {
        _value = value;

        // [CHANGED] tự debounce ở đây (không dựa MudTextField DebounceInterval)
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();

        var token = _debounceCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DebounceInterval, token);

                // Sau debounce mới update Store (không sync URL)
                QueryStore.UpdateActive(q =>
                {
                    q.Search = string.IsNullOrWhiteSpace(_value) ? null : _value!.Trim();
                    q.Page = 0;
                }, SearchQueryChangeSource.Ui);
            }
            catch (OperationCanceledException) { }
        }, token);

        return Task.CompletedTask;
    }

    private Task OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            // [CHANGED] Enter = hủy debounce pending + commit ngay
            _debounceCts?.Cancel();

            CommitSearchFromTextbox();
            SyncUrlFromActiveQuery(replaceHistory: true);
            return Task.CompletedTask;
        }

        if (e.Key == "Escape")
        {
            _debounceCts?.Cancel();

            _value = string.Empty;
            QueryStore.UpdateActive(q =>
            {
                q.Search = null;
                q.Page = 0;
            }, SearchQueryChangeSource.Ui);

            SyncUrlFromActiveQuery(replaceHistory: true);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
    private Task OnClearButtonClicked(MouseEventArgs args)
    {
        _value = string.Empty;
        QueryStore.UpdateActive(q =>
        {
            q.Search = null;
            q.Page = 0;
        }, SearchQueryChangeSource.Ui);

        SyncUrlFromActiveQuery(replaceHistory: true);

        return Task.CompletedTask;
    }
    private void CommitSearchFromTextbox()
    {
        var keyword = (_value ?? string.Empty).Trim();

        QueryStore.UpdateActive(q =>
        {
            q.Search = string.IsNullOrWhiteSpace(keyword) ? null : keyword;
            q.Page = 0;
        }, SearchQueryChangeSource.Ui);
    }

    private void SyncUrlFromActiveQuery(bool replaceHistory)
    {
        if (string.IsNullOrWhiteSpace(QueryStore.ActiveKey))
            return;

        var q = QueryStore.GetActive();

        // Giữ URL gọn: bỏ param nếu là default/null
        var uri = NavigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["page"] = q.Page == 0 ? null : q.Page,
            ["pageSize"] = q.PageSize == 20 ? null : q.PageSize,
            ["search"] = string.IsNullOrWhiteSpace(q.Search) ? null : q.Search,
            ["sortBy"] = string.IsNullOrWhiteSpace(q.SortBy) ? null : q.SortBy,
            ["sortDesc"] = q.SortDesc ? true : null
        });

        NavigationManager.NavigateTo(uri, replace: replaceHistory);
    }

    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();

        QueryStore.ActiveKeyChanged -= OnActiveKeyChanged;
        QueryStore.Changed -= OnQueryChanged;
    }
}
