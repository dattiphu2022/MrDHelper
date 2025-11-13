using System.Globalization;

namespace MrDHelper.ValueTypeHelpers.DateTimeHelper;

public static class DateTimeHelper
{
    private static readonly TimeZoneInfo _vietnamZone =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public static string ToVietnamString(this DateTimeOffset utc, string? format = null)
    {
        var vn = TimeZoneInfo.ConvertTime(utc, _vietnamZone);
        return vn.ToString(format ?? "dd/MM/yyyy HH:mm:ss", new CultureInfo("vi-VN"));
    }
}
