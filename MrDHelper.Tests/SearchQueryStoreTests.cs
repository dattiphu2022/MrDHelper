using MrDHelper.MudBlazor.Search;

namespace MrDHelper.Tests;

[TestFixture]
public class SearchQueryStoreTests
{
    [Test]
    public void Normalize_ShouldTrimClampAndCleanFilters()
    {
        var query = new SearchQuery
        {
            Page = -3,
            PageSize = 999,
            Search = "  keyword  ",
            SortBy = "  name  ",
            Filters = new Dictionary<string, string?>
            {
                [" status "] = "  active  ",
                ["empty"] = "   ",
                ["   "] = "ignored"
            }
        };

        query.Normalize();

        Assert.Multiple(() =>
        {
            Assert.That(query.Page, Is.EqualTo(0));
            Assert.That(query.PageSize, Is.EqualTo(200));
            Assert.That(query.Search, Is.EqualTo("keyword"));
            Assert.That(query.SortBy, Is.EqualTo("name"));
            Assert.That(query.Filters.Keys, Is.EquivalentTo(new[] { "status", "empty" }));
            Assert.That(query.Filters["status"], Is.EqualTo("active"));
            Assert.That(query.Filters["empty"], Is.Null);
        });
    }

    [Test]
    public void Clone_ShouldCopyValues_ButUseNewIdentity()
    {
        var original = new SearchQuery
        {
            Page = 2,
            PageSize = 50,
            Search = "abc",
            SortBy = "Title",
            SortDesc = true,
            Filters = new Dictionary<string, string?> { ["status"] = "open" }
        };

        var clone = original.Clone();
        clone.Filters["status"] = "closed";

        Assert.Multiple(() =>
        {
            Assert.That(clone.Id, Is.Not.EqualTo(original.Id));
            Assert.That(clone.Page, Is.EqualTo(original.Page));
            Assert.That(clone.PageSize, Is.EqualTo(original.PageSize));
            Assert.That(clone.Search, Is.EqualTo(original.Search));
            Assert.That(clone.SortBy, Is.EqualTo(original.SortBy));
            Assert.That(clone.SortDesc, Is.EqualTo(original.SortDesc));
            Assert.That(original.Filters["status"], Is.EqualTo("open"));
            Assert.That(clone.Filters["status"], Is.EqualTo("closed"));
        });
    }

    [Test]
    public void SetActiveKey_ShouldRaiseEventOnlyWhenValueChanges()
    {
        var store = new SearchQueryStore();
        var events = new List<string>();

        store.ActiveKeyChanged += key => events.Add(key);

        store.SetActiveKey("users");
        store.SetActiveKey("USERS");
        store.SetActiveKey("reports");

        Assert.Multiple(() =>
        {
            Assert.That(store.ActiveKey, Is.EqualTo("reports"));
            Assert.That(events, Is.EqualTo(new[] { "users", "reports" }));
        });
    }

    [Test]
    public void Set_ShouldNormalizePayloadAndSkipEquivalentUpdates()
    {
        var store = new SearchQueryStore();
        SearchQueryChangedArgs? changed = null;

        store.Changed += args => changed = args;

        var updated = store.Set(
            "users",
            new SearchQuery
            {
                Page = -10,
                PageSize = 500,
                Search = "  abc  ",
                Filters = new Dictionary<string, string?> { [" status "] = "  active  " }
            },
            SearchQueryChangeSource.Ui);

        var skipped = store.Set(
            "users",
            new SearchQuery
            {
                Page = 0,
                PageSize = 200,
                Search = "abc",
                Filters = new Dictionary<string, string?> { ["status"] = "active" }
            },
            SearchQueryChangeSource.Ui);

        var stored = store.Get("users");

        Assert.Multiple(() =>
        {
            Assert.That(updated, Is.True);
            Assert.That(skipped, Is.False);
            Assert.That(changed, Is.Not.Null);
            Assert.That(changed!.Source, Is.EqualTo(SearchQueryChangeSource.Ui));
            Assert.That(changed.Query.Search, Is.EqualTo("abc"));
            Assert.That(stored.Page, Is.EqualTo(0));
            Assert.That(stored.PageSize, Is.EqualTo(200));
            Assert.That(stored.Filters["status"], Is.EqualTo("active"));
        });
    }

    [Test]
    public void UpdateActive_WithoutExplicitActiveKey_ShouldUseDefaultBucket()
    {
        var store = new SearchQueryStore();

        var changed = store.UpdateActive(query =>
        {
            query.Search = "  pending  ";
            query.Page = -5;
        }, SearchQueryChangeSource.Code);

        var active = store.GetActive();

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.True);
            Assert.That(active.Search, Is.EqualTo("pending"));
            Assert.That(active.Page, Is.EqualTo(0));
        });
    }
}
