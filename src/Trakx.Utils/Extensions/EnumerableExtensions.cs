using System.Security.Cryptography;
using MathNet.Numerics.Statistics;

namespace Trakx.Utils.Extensions;

public static class EnumerableExtensions
{
    public static IList<T> Shuffle<T>(this IEnumerable<T> enumerable)
    {
        var list = enumerable.ToList();
        var count = list.Count;
        while (count > 1)
        {
            var box = new byte[1];
            do box[0] = RandomNumberGenerator.GetBytes(1)[0];
            while (box[0] >= count * (byte.MaxValue / count));
            var k = (box[0] % count);
            count--;
            (list[k], list[count]) = (list[count], list[k]);
        }

        return list;
    }

    public static IEnumerable<T> IntersectMany<T>(this IEnumerable<IEnumerable<T>> enumOfEnums)
    {
        var ofEnums = enumOfEnums as List<IEnumerable<T>> ?? enumOfEnums.ToList();
        if (!ofEnums.Any()) return Enumerable.Empty<T>();
        var intersection = ofEnums
            .Skip(1)
            .Aggregate(
                new HashSet<T>(ofEnums.First()),
                (h, e) =>
                {
                    h.IntersectWith(e);
                    return h;
                }
            );
        return intersection;
    }

    public readonly struct SelectionWithStatistics<T>
    {
        public SelectionWithStatistics(T? selection, double mean, double standardDeviation, double median)
        {
            Selection = selection;
            Mean = mean;
            StandardDeviation = standardDeviation;
            Median = median;
        }

        public readonly T? Selection;
        public readonly double Mean;
        public readonly double Median;
        public readonly double StandardDeviation;
    }

    /// <summary>
    /// Returns the first value, in order of preference, of a given list of <see cref="T"/>, where <see cref="T"/>
    /// are measured by the value returned by the <see cref="valueSelector"/>.
    /// Useful for choosing a price from a list of quotes by different market data providers, one of which can
    /// return an erroneous quote.
    /// If no match is found below <see cref="maxStandardDeviations"/>, the one with the lowest standard deviation is returned.
    /// </summary>
    /// <typeparam name="T">Type of the items that are being sorter by preference.</typeparam>
    /// <param name="preferences">List of items to choose from, ordered by preference.</param>
    /// <param name="valueSelector">Function used to select the value on which to base the statistics used to pick a value.</param>
    /// <param name="maxStandardDeviations">Maximum number of standard deviation on which to base the selection. Beyond that value, a
    /// preferred value is rejected and the next preferred one will be evaluated.</param>
    /// <param name="useDeviationFromMedian">Calculate deviation from the median rather than the mean.</param>
    /// <param name="throwIfNoMatchFound">False by default, allows the method to throw if no match under <see cref="maxStandardDeviations"/>
    /// is found.</param>
    internal static SelectionWithStatistics<T?> SelectPreferenceWithMaxDeviationThreshold<T>(this IEnumerable<T> preferences,
        Func<T, double?> valueSelector, double maxStandardDeviations = 0.2, bool useDeviationFromMedian = false, bool throwIfNoMatchFound = false)
    {
        var preferenceList = preferences.Where(p => !double.IsNaN(valueSelector(p) ?? double.NaN)).ToList();

        var values = preferenceList.Select(v => valueSelector(v)!.Value).ToList();

        if (preferenceList.Count == 1)
            return new SelectionWithStatistics<T?>(preferenceList[0], values[0], 0, values[0]);

        var (mean, standardDeviation) = values.MeanStandardDeviation();
        var median = values.Median();
        if(double.IsNaN(mean) || double.IsNaN(standardDeviation) || double.IsNaN(median))
            return new SelectionWithStatistics<T?>(default, mean, standardDeviation, median);

        var minimumDeviation = double.MaxValue;
        var leastDeviated = default(T?);
        foreach (var preference in preferenceList)
        {
            var deviation = Math.Abs(valueSelector(preference)!.Value - (useDeviationFromMedian ? median : mean));
            if (deviation < maxStandardDeviations * standardDeviation)
                return new SelectionWithStatistics<T?>(preference, mean, standardDeviation, median);

            if (deviation > minimumDeviation) continue;
            minimumDeviation = deviation;
            leastDeviated = preference;
        }

        return throwIfNoMatchFound
            ? throw new InvalidDataException(
                $"Failed to find a valid value from list within {maxStandardDeviations} " +
                $"standard deviations of the mean, with mean {mean} and standardDeviation {standardDeviation}")
            : new SelectionWithStatistics<T?>(leastDeviated, mean, standardDeviation, median);
    }

    /// <summary>
    /// Returns the value with the lowest standard deviation from a given distribution.
    /// </summary>
    /// <typeparam name="T">Type of the items that are being sorter by preference.</typeparam>
    /// <param name="preferences">List of items to choose from, ordered by preference.</param>
    /// <param name="valueSelector">Function used to select the value on which to base the statistics used to pick a value.</param>
    public static SelectionWithStatistics<T?> SelectLeastDeviatedFromMeanValue<T>(this IEnumerable<T> preferences,
        Func<T, double?> valueSelector)
    {
        return SelectPreferenceWithMaxDeviationThreshold(preferences, valueSelector, 0);
    }

    /// <summary>
    /// Returns the value which is closest to the median of a given distribution.
    /// </summary>
    /// <typeparam name="T">Type of the items that are being sorter by preference.</typeparam>
    /// <param name="preferences">List of items to choose from, ordered by preference.</param>
    /// <param name="valueSelector">Function used to select the value on which to base the statistics used to pick a value.</param>
    public static SelectionWithStatistics<T?> SelectLeastDeviatedFromMedianValue<T>(this IEnumerable<T> preferences,
        Func<T, double?> valueSelector)
    {
        return SelectPreferenceWithMaxDeviationThreshold(preferences, valueSelector, 0, true);
    }

    public static string ToCsvList<T>(this IEnumerable<T> items,
        bool distinct = false,
        bool toLower = false,
        bool spacing = false,
        bool trim = true,
        bool quoted = true)
    {
        var stringItems = items.Select(i =>
        {
            var result = i?.ToString() ?? "";
            if (toLower) result = result.ToLowerInvariant();
            if (trim) result = result.Trim(' ');
            if (quoted) result = $"\"{result}\"";
            return result;
        });
        if (distinct) stringItems = stringItems.Distinct();
        var separator = "," + (spacing ? " " : string.Empty);
        return string.Join(separator, stringItems);
    }

}
