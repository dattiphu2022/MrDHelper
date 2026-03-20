using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MrDHelper.AppData.Extensions;

namespace MrDHelper.Tests;

[TestFixture]
public class QueryableExtensionsTests
{
    private SqliteConnection _connection = null!;
    private SearchDbContext _db = null!;

    [SetUp]
    public void SetUp()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SearchDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new SearchDbContext(options);
        _db.Database.EnsureCreated();
        _db.Items.AddRange(
            new SearchItem { Id = 1, Title = "Ha Noi Center", Summary = "alpha district" },
            new SearchItem { Id = 2, Title = "Sai Gon", Summary = "H\u00e0 N\u1ed9i riverside" },
            new SearchItem { Id = 3, Title = "Hue", Summary = "beta archive" });
        _db.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Test]
    public void ApplySearchClientSide_ShouldMatchNormalizedKeywordAcrossProperties()
    {
        var resultIds = _db.Items
            .AsNoTracking()
            .ApplySearchClientSide("  ha noi  ", item => item.Title, item => item.Summary)
            .OrderBy(item => item.Id)
            .Select(item => item.Id)
            .ToList();

        Assert.That(resultIds, Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void ApplySearchAnyClientSide_ShouldMatchAnyNormalizedToken()
    {
        var resultIds = _db.Items
            .AsNoTracking()
            .ApplySearchAnyClientSide("beta; center", item => item.Title, item => item.Summary)
            .OrderBy(item => item.Id)
            .Select(item => item.Id)
            .ToList();

        Assert.That(resultIds, Is.EqualTo(new[] { 1, 3 }));
    }

    [Test]
    public async Task ApplySearchAnyServerSide_ShouldTranslateToLikeAcrossProperties()
    {
        var resultIds = await _db.Items
            .AsNoTracking()
            .ApplySearchAnyServerSide("beta", item => item.Title, item => item.Summary)
            .OrderBy(item => item.Id)
            .Select(item => item.Id)
            .ToListAsync();

        Assert.That(resultIds, Is.EqualTo(new[] { 3 }));
    }

    [Test]
    public void ApplySearchAnyServerSide_NullSource_ShouldThrowArgumentNullException()
    {
        IQueryable<SearchItem>? source = null;

        Assert.Throws<ArgumentNullException>(() =>
            source!.ApplySearchAnyServerSide("beta", item => item.Title));
    }

    [Test]
    public async Task ToPagedAsync_ShouldClampInvalidArguments_AndPopulateMetadata()
    {
        var result = await _db.Items
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .ToPagedAsync(page: -2, pageSize: 0);

        Assert.Multiple(() =>
        {
            Assert.That(result.Page, Is.EqualTo(0));
            Assert.That(result.PageSize, Is.EqualTo(20));
            Assert.That(result.Total, Is.EqualTo(3));
            Assert.That(result.Items.Select(item => item.Id), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(result.PageCount, Is.EqualTo(1));
            Assert.That(result.HasPreviousPage, Is.False);
            Assert.That(result.HasNextPage, Is.False);
            Assert.That(result.FirstItemIndex, Is.EqualTo(1));
            Assert.That(result.LastItemIndex, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task ToPagedSliceAsync_ShouldSetHasMoreAndTrimExtraRows()
    {
        var result = await _db.Items
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .ToPagedSliceAsync(page: 0, pageSize: 2);

        Assert.Multiple(() =>
        {
            Assert.That(result.HasMore, Is.True);
            Assert.That(result.Page, Is.EqualTo(0));
            Assert.That(result.PageSize, Is.EqualTo(2));
            Assert.That(result.Items.Select(item => item.Id), Is.EqualTo(new[] { 1, 2 }));
        });
    }

    private sealed class SearchDbContext(DbContextOptions<SearchDbContext> options) : DbContext(options)
    {
        public DbSet<SearchItem> Items => Set<SearchItem>();
    }

    private sealed class SearchItem
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Summary { get; set; }
    }
}
