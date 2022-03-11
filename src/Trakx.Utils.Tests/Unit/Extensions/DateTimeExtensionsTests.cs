using System;
using System.Linq;
using FluentAssertions;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions;

public class DateTimeExtensionsTests
{
    [Fact]
    public void GetLondonClosingTimeForDay_should_handle_daylight_savings()
    {
        var daylightSavingStart = new DateTime(2020, 03, 29, 0, 0, 0, DateTimeKind.Utc);
        var startTime = daylightSavingStart.AddDays(-2);
        var endTime = daylightSavingStart.AddDays(2);
        var distanceInHours = (int)(endTime - startTime).TotalHours + 1;
        var oneHour = TimeSpan.FromHours(1);

        var realHours = Enumerable.Range(0, distanceInHours)
            .Select(i => startTime.Add(oneHour.Multiply(i))).ToList();

        var effectiveHours =
            realHours.Select(h => new {DateTime = h, Closing = h.GetLondonClosingTimeForDay()})
                .ToList();

        effectiveHours.Count(k => k.Closing.TimeOfDay.Equals(TimeSpan.FromHours(17))).Should().Be(24 * 2 + 1);
        effectiveHours.Count(k => k.Closing.TimeOfDay.Equals(TimeSpan.FromHours(18))).Should().Be(24 * 2);
    }

    [Fact]
    public void Round_should_round_to_nearest_value()
    {
        var offset = TimeSpan.FromTicks(1);
        var dateTime = new DateTimeOffset().Add(offset);

        dateTime.Round(TimeSpan.FromMinutes(1)).Should().Be(new DateTimeOffset());
        dateTime.Add(TimeSpan.FromSeconds(30)).Round(TimeSpan.FromMinutes(1)).Should().Be(new DateTimeOffset().Add(TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public void GetDatesUntil_should_return_round_dates_until_endTime()
    {
        var start = DateTimeOffset.Parse("2021-05-01T22:34:44z");
        var end = DateTimeOffset.Parse("2021-05-04T21:11:11z");

        start.GetDatesUntil(end).Should().BeEquivalentTo(new []
        {
            DateTimeOffset.Parse("2021-05-01Z"),
            DateTimeOffset.Parse("2021-05-02Z"),
            DateTimeOffset.Parse("2021-05-03Z"),
            DateTimeOffset.Parse("2021-05-04Z"),
        });

        end.GetDatesUntil(start).Should().BeEmpty();
    }

    [Fact]
    public void GetDatesUntil_should_return_single_date()
    {
        var start = DateTimeOffset.Parse("2021-05-01T21:34:44z");
        var end = DateTimeOffset.Parse("2021-05-01T22:11:11z");

        start.GetDatesUntil(end).Should().BeEquivalentTo(new[]
        {
            DateTimeOffset.Parse("2021-05-01Z")
        });

        end.GetDatesUntil(start).Should().BeEmpty();
    }

    [Fact]
    public void GetDatesUntil_strict_should_remove_last_date_if_same_as_enddate()
    {
        var start = DateTimeOffset.Parse("2021-05-01T21:34:44z");
        var end = DateTimeOffset.Parse("2021-05-03T00:00:00z");

        start.GetDatesUntil(end).Should().BeEquivalentTo(new[]
        {
            DateTimeOffset.Parse("2021-05-01Z"),
            DateTimeOffset.Parse("2021-05-02Z")
        });

        start.GetDatesUntil(end, false).Should().BeEquivalentTo(new[]
        {
            DateTimeOffset.Parse("2021-05-01Z"),
            DateTimeOffset.Parse("2021-05-02Z"),
            DateTimeOffset.Parse("2021-05-03Z")
        });

        end.GetDatesUntil(start).Should().BeEmpty();
    }


    [Theory]
    [InlineData("2021-05-01Z", "2021-05-01Z")]
    [InlineData("2021-05-01T15:34:40Z", "2021-05-01Z")]
    [InlineData("2021-05-01T04:30:40+05:30", "2021-04-30Z")]
    [InlineData("2021-05-01T18:30:40-05:30", "2021-05-02Z")]
    [InlineData("2021-05-01T18:40:40-05:30", "2021-05-02Z")]
    public void DateUtc_should_return_single_date(string dateTime, string expectedDate)
    {
        var input = DateTimeOffset.Parse(dateTime);
        var expectedOutput = DateTimeOffset.Parse(expectedDate);

        input.UtcDate().Should().Be(expectedOutput);

    }
}