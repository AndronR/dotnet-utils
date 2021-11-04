using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using MathNet.Numerics.Statistics;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions
{
    public class EnumerableExtensionsTests
    {
        private readonly double[] _distribution;

        public EnumerableExtensionsTests()
        {
            _distribution = new[] { 1.3, 1.2, 1.0, 0.9, 0.8, 0.8, 1.2 };
        }

        private class ObjectToCompare
        {
            public string? Name { get; init; }
            public string? Reference { get; init; }
        }

        [Fact]
        public void DistinctBy_should_compare_items_using_selector()
        {
            var classes = new[]
            {
                new ObjectToCompare {Name = "abc", Reference = "123"},
                new ObjectToCompare {Name = "def", Reference = "123"},
                new ObjectToCompare {Name = "ghi", Reference = "123"},
                new ObjectToCompare {Name = "abc", Reference = "456"},
            };

            classes.DistinctBy(c => c!.Name).Should().BeEquivalentTo(classes.Take(3));
            classes.DistinctBy(c => c!.Reference).Should().BeEquivalentTo(new[] { classes[0]!, classes[3]! });
        }

        [Fact]
        public void Shuffle_should_randomise_collection_items()
        {
            var ordered = Enumerable.Range(0, 40).ToList();
            for (var i = 0; i < 20; i++)
            {
                var shuffled1 = ordered.Shuffle();
                shuffled1.Should().BeEquivalentTo(ordered);
                string.Join(" ", ordered).Should()
                    .NotBe(string.Join(" ", shuffled1));

                var shuffled2 = ordered.Shuffle();
                shuffled2.Should().BeEquivalentTo(ordered);
                string.Join(" ", ordered).Should()
                    .NotBe(string.Join(" ", shuffled2));

                string.Join(" ", shuffled1).Should()
                    .NotBe(string.Join(" ", shuffled2));
            }
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_choose_first_value_if_not_too_deviant()
        {
            var (mean, standardDeviation) = _distribution.MeanStandardDeviation();

            Math.Abs(_distribution[0] - mean).Should().BeLessOrEqualTo(3 * standardDeviation);

            var selection = _distribution.SelectPreferenceWithMaxDeviationThreshold(x => x, 3);

            selection.Selection.Should().Be(_distribution[0]);
            selection.Mean.Should().BeApproximately(1.0285714285714285, double.Epsilon);
            selection.StandardDeviation.Should().BeApproximately(0.20586634591635514, double.Epsilon);
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_choose_first_values_if_too_deviant()
        {
            var (mean, standardDeviation) = _distribution.MeanStandardDeviation();
            var maxStandardDeviation = 0.5;

            Math.Abs(_distribution[0] - mean).Should().BeGreaterOrEqualTo(standardDeviation * maxStandardDeviation);
            Math.Abs(_distribution[1] - mean).Should().BeGreaterOrEqualTo(standardDeviation * maxStandardDeviation);

            var selection = _distribution.SelectPreferenceWithMaxDeviationThreshold(x => x, maxStandardDeviation);

            selection.Selection.Should().Be(_distribution[2]);
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_throw_when_asked_and_no_value_matches()
        {
            var maxStandardDeviation = GetTooStrictMaxStandardDeviation();

            var selectionAction = new Action(() =>
                _distribution.SelectPreferenceWithMaxDeviationThreshold(x => x, maxStandardDeviation, throwIfNoMatchFound: true));

            selectionAction.Should().Throw<InvalidDataException>();
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_not_throw_when_not_asked_and_no_value_matches()
        {
            var maxStandardDeviation = GetTooStrictMaxStandardDeviation();

            var selectionAction = new Func<EnumerableExtensions.SelectionWithStatistics<double?>>(
                () => _distribution.Select(x => new double?(x))
                    .SelectPreferenceWithMaxDeviationThreshold(x => x!.Value, maxStandardDeviation));

            selectionAction.Should().NotThrow<Exception>();
            selectionAction.Invoke().Selection.Should().Be(1);
        }

        private double GetTooStrictMaxStandardDeviation()
        {
            var (mean, standardDeviation) = _distribution.MeanStandardDeviation();
            var maxStandardDeviation = 0.01;

            _distribution.Select(x => Math.Abs(x - mean)).All(d => d > standardDeviation * maxStandardDeviation)
                .Should().BeTrue();
            return maxStandardDeviation;
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_not_throw_on_empty_sets()
        {
            var selectionAction = new Func<EnumerableExtensions.SelectionWithStatistics<double?>>(
                () => Array.Empty<double?>().SelectPreferenceWithMaxDeviationThreshold(x => x ?? 0, 10));

            selectionAction.Should().NotThrow<Exception>();
            selectionAction.Invoke().Selection.Should().Be(null);
        }


        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_not_throw_on_sets_with_one_value()
        {
            var selectionAction = new Func<EnumerableExtensions.SelectionWithStatistics<double?>>(
                () => new double?[] { 0.245, double.NaN, null }.SelectPreferenceWithMaxDeviationThreshold(
                    x => x ?? 0, 10));

            selectionAction.Should().NotThrow<Exception>();
            selectionAction.Invoke().Selection.Should().Be(0.245);
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_not_throw_on_sets_with_two_values()
        {
            var selectionAction = new Func<EnumerableExtensions.SelectionWithStatistics<double?>>(
                () => new double?[] { 0.245, 0.256, null }.SelectPreferenceWithMaxDeviationThreshold(
                    x => x ?? 0, 10));

            selectionAction.Should().NotThrow<Exception>();
            selectionAction.Invoke().Selection.Should().Be(0.245);
        }

        [Fact]
        public void ToCsvDistinctList_should_join_trimmed_lower_cased_ToString_results_with_spacing()
        {
            var strings = new[] { "ab ", "def", " klm", "KlM" };
            strings.ToCsvList(true, true, true, quoted: false).Should().Be("ab, def, klm");
            strings.ToCsvList(true, false, true, quoted: true, trim: false)
                .Should().Be("\"ab \", \"def\", \" klm\", \"KlM\"");
        }

        [Fact]
        public void SelectLeastDeviatedFromMeanValue_example_from_real_life_should_not_pick_cryptocompare_valution()
        {
            var values = new Dictionary<string, decimal>
            {
                {"CryptoCompare", 0.004720472047204721m},
                {"Shrimpy", 0.6387407104069606m},
                {"CoinGecko", 0.6526023201009594m},
            };

            var selection = values.SelectLeastDeviatedFromMeanValue(p => (double)p.Value);
            selection.Selection.Key.Should().NotBe("CryptoCompare");
        }

        [Fact]
        public void SelectLeastDeviatedFromMedianValue_return_the_first_value_closest_to_the_median()
        {
            var values = new Dictionary<string, decimal>
            {
                {"CryptoCompare", 0.001m},
                {"Coingecko", 0.001m},
                {"Shrimpy", 1.2m},
                {"Binance", 1.3m},
                {"Okex", 1.4m},
                {"Gemini", 1.5m},
                {"BitStamp", 1.6m},
                {"Kucoin", 1.7m},
            };

            var selection = values.SelectLeastDeviatedFromMedianValue(p => (double)p.Value);
            selection.Median.Should().Be(1.35);
            selection.Mean.Should().Be(1.08775);
            selection.Selection.Key.Should().Be("Okex");
        }
    }
}