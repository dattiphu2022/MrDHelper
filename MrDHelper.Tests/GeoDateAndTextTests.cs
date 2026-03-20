using MrDHelper;
using MrDHelper.Models;
using MrDHelper.ValueTypeHelpers.DateTimeHelper;
using MrDHelper.ValueTypeHelpers.GeoPointHelper;
using System.Globalization;

namespace MrDHelper.Tests;

[TestFixture]
public class GeoDateAndTextTests
{
    [Test]
    public void ToVietnamString_ShouldConvertUtcToVietnamTime()
    {
        var utc = new DateTimeOffset(2026, 3, 20, 0, 30, 0, TimeSpan.Zero);

        var result = utc.ToVietnamString();

        Assert.That(result, Is.EqualTo("20/03/2026 07:30:00"));
    }

    [Test]
    public void Normalize_ShouldRemoveVietnameseDiacritics_AndTrimWhitespace()
    {
        var normalized = StringHelper.Normalize("  \u0110\u1eb7ng   Th\u1ecb  H\u00e0 N\u1ed9i  ");

        Assert.That(normalized, Is.EqualTo("dang   thi  ha noi"));
    }

    [Test]
    public void ToReadableDistance_ShouldFormatMetersAndKilometers()
    {
        Assert.Multiple(() =>
        {
            Assert.That(125.4d.ToReadableDistance(), Is.EqualTo("125 m"));
            Assert.That(1_540d.ToReadableDistance(), Is.EqualTo("1.5 km"));
            Assert.That(12_500d.ToReadableDistance(integerOnly: true), Is.EqualTo("12 km"));
        });
    }

    [Test]
    public void ToCompactDistance_ShouldRoundToUiFriendlyText()
    {
        Assert.Multiple(() =>
        {
            Assert.That(299.6d.ToCompactDistance(), Is.EqualTo("300m"));
            Assert.That(1_540d.ToCompactDistance(), Is.EqualTo("1.5km"));
        });
    }

    [Test]
    public void DistanceFormatting_ShouldUseInvariantDecimalSeparator()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            var vietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");
            CultureInfo.CurrentCulture = vietnameseCulture;
            CultureInfo.CurrentUICulture = vietnameseCulture;

            Assert.Multiple(() =>
            {
                Assert.That(1_540d.ToReadableDistance(), Is.EqualTo("1.5 km"));
                Assert.That(1_540d.ToCompactDistance(), Is.EqualTo("1.5km"));
            });
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Test]
    public void DistanceHelpers_WithSamePoint_ShouldReturnZero()
    {
        var point = new GeoPoint(10.0, 106.0);

        Assert.Multiple(() =>
        {
            Assert.That(point.ApproxDistanceMetersTo(point), Is.EqualTo(0d).Within(0.001d));
            Assert.That(point.HaversineDistanceMetersTo(point), Is.EqualTo(0d).Within(0.001d));
        });
    }

    [Test]
    public void BearingDegreesTo_EastwardPoint_ShouldBeCloseToNinety()
    {
        var from = new GeoPoint(0, 0);
        var to = new GeoPoint(0, 1);

        var bearing = from.BearingDegreesTo(to);

        Assert.That(bearing, Is.EqualTo(90d).Within(0.5d));
    }
}
