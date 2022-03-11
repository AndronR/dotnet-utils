using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Trakx.Utils.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Tries to find the uK timezone - This code is unfortunately platform dependent.
    /// </summary>
    private static readonly Lazy<TimeZoneInfo> LazyLondonTimeZone
        = new(() => TimeZoneInfo.GetSystemTimeZones().Single(t =>
                t.DisplayName.Contains("London") || t.Id.Contains("London") || t.DisplayName.Contains("Londres")),
            LazyThreadSafetyMode.PublicationOnly);

    public static TimeZoneInfo LondonTimeZone => LazyLondonTimeZone.Value;

    public static string ToIso8601(this DateTime dateTime, bool asUtc)
        => (asUtc ? dateTime.ToUniversalTime() : dateTime).ToString("o", CultureInfo.InvariantCulture);

    public static ulong ToUnixDateTime(this DateTime utcDateTime)
    {
        return (ulong) new DateTimeOffset(utcDateTime).ToUnixTimeSeconds();
    }

    public static ulong ToUnixDateTimeMilliseconds(this DateTime utcDateTime)
    {
        return (ulong)new DateTimeOffset(utcDateTime).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Returns London closing date and time for a given date, in UTC.
    /// </summary>
    /// <param name="day">The day for which the closing time is requested.</param>
    public static DateTime GetLondonClosingTimeForDay(this DateTime day)
    {
        var londonCloseUtc = TimeZoneInfo.ConvertTime(
            new DateTime(day.Year, day.Month, day.Day, 18, 0, 0, DateTimeKind.Unspecified),
            LondonTimeZone, TimeZoneInfo.Utc);
        return londonCloseUtc;
    }
}