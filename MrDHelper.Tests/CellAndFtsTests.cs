using MrDHelper.AppDomain.EfSqliteFts5;
using MrDHelper.CellHelpers;
using MrDHelper.Models;

namespace MrDHelper.Tests;

[TestFixture]
public class CellAndFtsTests
{
    [Test]
    public void CellIndexer_MissingProperty_ShouldReturnNull()
    {
        var cell = new Cell();

        Assert.That(cell["Missing"], Is.Null);
    }

    [Test]
    public void ConvertToCell_AndBack_ShouldRoundTripKnownProperties()
    {
        var source = new DummyCellModel
        {
            Name = "Dat",
            Age = 30
        };

        var cell = source.ConvertToCell() ?? throw new AssertionException("Expected a cell instance.");
        var restored = cell.ConvertTo<DummyCellModel>();
        var restoredModel = restored ?? throw new AssertionException("Expected a restored model instance.");

        Assert.Multiple(() =>
        {
            Assert.That(cell["Name"], Is.EqualTo("Dat"));
            Assert.That(cell["Age"], Is.EqualTo(30));
            Assert.That(restored, Is.Not.Null);
            Assert.That(restoredModel.Name, Is.EqualTo("Dat"));
            Assert.That(restoredModel.Age, Is.EqualTo(30));
        });
    }

    [Test]
    public void VietFtsNormalize_ShouldRemoveDiacriticsAndCollapseSpaces()
    {
        var normalized = VietFts.Normalize("  \u0110\u1eb7ng   Th\u1ecb   H\u00e0  ");

        Assert.That(normalized, Is.EqualTo("dang thi ha"));
    }

    [Test]
    public void VietFtsBuildMatchQuery_ShouldEscapeInputAndJoinTokens()
    {
        var query = VietFts.BuildMatchQuery(" H\u00e0-N\u1ed9i, e30 ", prefix: true);

        Assert.That(query, Is.EqualTo("ha* AND noi* AND e30*"));
    }

    [Test]
    public void FtsRegistry_Register_ShouldExposeRegisteredSpec()
    {
        FtsRegistry.Register<DummyFtsEntity>("Reports", "ReportsFts");

        var found = FtsRegistry.TryGet<DummyFtsEntity>(out var spec);

        Assert.Multiple(() =>
        {
            Assert.That(found, Is.True);
            Assert.That(spec.EntityName, Is.EqualTo(nameof(DummyFtsEntity)));
            Assert.That(spec.MainTable, Is.EqualTo("Reports"));
            Assert.That(spec.FtsTable, Is.EqualTo("ReportsFts"));
            Assert.That(spec.IdColumn, Is.EqualTo("Id"));
        });
    }

    private sealed class DummyCellModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private sealed class DummyFtsEntity : IHasGuidId, IFtsIndexed
    {
        public Guid Id { get; set; }

        public string BuildFtsAllText() => "dummy";
    }
}
