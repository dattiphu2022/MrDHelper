using System;
using System.Globalization;

namespace MrDHelper.ValueTypeHelpers.DateTimeHelper;

public static class DateTimeHelper
{
    private static readonly Lazy<TimeZoneInfo> _vietnamZone = new(CreateVietnamZone);

    public static string ToVietnamString(this DateTimeOffset utc, string? format = null)
    {
        var vn = TimeZoneInfo.ConvertTime(utc, _vietnamZone.Value);
        return vn.ToString(format ?? "dd/MM/yyyy HH:mm:ss", new CultureInfo("vi-VN"));
    }

    private static TimeZoneInfo CreateVietnamZone()
    {
        foreach (var timeZoneId in new[] { "SE Asia Standard Time", "Asia/Ho_Chi_Minh" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.CreateCustomTimeZone(
            "Asia/Ho_Chi_Minh",
            TimeSpan.FromHours(7),
            "Vietnam Time",
            "Vietnam Time");
    }
}
