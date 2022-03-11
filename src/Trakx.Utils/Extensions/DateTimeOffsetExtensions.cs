using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Trakx.Utils.DateTimeHelpers;

namespace Trakx.Utils.Extensions;

public static class DateTimeOffsetExtensions
{
    public static string ToIso8601(this DateTimeOffset dateTime, bool asUtc = true)
        => (asUtc ? dateTime.ToUniversalTime() : dateTime).ToString("o", CultureInfo.InvariantCulture);

    public static DateTimeOffset Round(this DateTimeOffset offset, TimeSpan timeSpan)
    {
        var offsetUtcDateTime = offset.UtcDateTime;
        var roundDateTime =
            new DateTime((offsetUtcDateTime.Ticks + timeSpan.Ticks / 2 - 1) / timeSpan.Ticks * timeSpan.Ticks,
                offsetUtcDateTime.Kind);
        return new DateTimeOffset(roundDateTime);
    }

    public static DateTimeOffset Round(this DateTimeOffset offset, Period period) =>
        Round(offset, period.ToTimeSpan());

    /// <summary>
    ///
    /// </summary>
    /// <param name="startDate">Date we start looking forward from.</param>
    /// <param name="endDate">Latest date at which we want to arrive.</param>
    /// <param name="strict">True by default, meaning that the last date of the interval will be removed if
    /// it is equal to <see cref="endDate"/>.</param>
    /// <returns></returns>
    public static List<DateTimeOffset> GetDatesUntil(this DateTimeOffset startDate, DateTimeOffset endDate,
        bool strict = true)
    {
        if (startDate > endDate) return new List<DateTimeOffset>();
        var count = 1 + endDate.UtcDate().Subtract(startDate.UtcDate()).Days;
        var dates = Enumerable.Range(0, count)
            .Select(offset => new DateTimeOffset(startDate.Date.AddDays(offset), TimeSpan.Zero))
            .ToList();
        if (strict && endDate == dates.Last()) dates.Remove(dates.Last());
        return dates;
    }

    /// <summary>
    /// Get the Date part of the Date time, represented in UTC.
    /// </summary>
    /// <param name="dateTime">DateTime for which we need the date part.</param>
    /// <returns>The UTC date as of <see cref="dateTime"/></returns>
    public static DateTimeOffset UtcDate(this DateTimeOffset dateTime) =>
        new(dateTime.UtcDateTime.Date, TimeSpan.Zero);
}