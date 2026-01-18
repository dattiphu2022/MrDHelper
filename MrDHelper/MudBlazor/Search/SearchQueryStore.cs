using System.Collections.Concurrent;

namespace MrDHelper.MudBlazor.Search;

public enum SearchQueryChangeSource
{
    Url,
    Ui,
    Code
}

public sealed class SearchQueryChangedArgs
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Key { get; set; } = string.Empty;
    public SearchQuery Query { get; set; } = new();
    public SearchQueryChangeSource Source { get; set; }
}

public sealed class SearchQueryStore
{
    public Guid Id { get; } = Guid.NewGuid();

    private readonly ConcurrentDictionary<string, SearchQuery> _store =
        new(StringComparer.OrdinalIgnoreCase);

    public string? ActiveKey { get; private set; }

    public event Action<string>? ActiveKeyChanged;
    public event Action<SearchQueryChangedArgs>? Changed;

    public void SetActiveKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        if (string.Equals(ActiveKey, key, StringComparison.OrdinalIgnoreCase))
            return;

        ActiveKey = key;
        Ensure(key);
        ActiveKeyChanged?.Invoke(key);
    }

    public SearchQuery Get(string key)
    {
        Ensure(key);
        return _store[key];
    }

    public SearchQuery GetActive()
    {
        var key = ActiveKey ?? "__none__";
        Ensure(key);
        return _store[key];
    }

    public bool Set(string key, SearchQuery next, SearchQueryChangeSource source)
    {
        Ensure(key);

        next.Normalize();

        var current = _store[key];
        if (current.EqualsValue(next))
            return false;

        _store[key] = next;

        Changed?.Invoke(new SearchQueryChangedArgs
        {
            Key = key,
            Query = next,
            Source = source
        });

        return true;
    }

    public bool Update(string key, Action<SearchQuery> mutate, SearchQueryChangeSource source)
    {
        Ensure(key);

        var clone = _store[key].Clone().Normalize();
        mutate(clone);
        return Set(key, clone, source);
    }

    public bool UpdateActive(Action<SearchQuery> mutate, SearchQueryChangeSource source)
    {
        var key = ActiveKey ?? "__none__";
        return Update(key, mutate, source);
    }

    private void Ensure(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            key = "__none__";

        _store.TryAdd(key, new SearchQuery().Normalize());
    }
}
