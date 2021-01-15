﻿using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using MathNet.Numerics.Statistics;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_choose_first_value_if_not_too_deviant()
        {
            var numbers = new [] {1.0, 1.3, 1.2, 0.9, 0.8, 0.8, 1.2};
            var (mean, standardDeviation) = numbers.MeanStandardDeviation();
            
            Math.Abs(numbers[0] - mean).Should().BeLessOrEqualTo(standardDeviation);

            var selection = numbers.SelectPreferenceWithMaxDeviationThreshold(x => x, 1);
            
            selection.Selection.Should().Be(numbers[0]);
            selection.Mean.Should().BeApproximately(1.0285714285714285, double.Epsilon);
            selection.StandardDeviation.Should().BeApproximately(0.20586634591635516, double.Epsilon);
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_choose_first_values_if_too_deviant()
        {
            var numbers = new [] { 1.3, 1.2, 1.0, 0.9, 0.8, 0.8, 1.2 };
            var (mean, standardDeviation) = numbers.MeanStandardDeviation();
            var maxStandardDeviation = 0.5;

            Math.Abs(numbers[0] - mean).Should().BeGreaterOrEqualTo(standardDeviation * maxStandardDeviation);
            Math.Abs(numbers[1] - mean).Should().BeGreaterOrEqualTo(standardDeviation * maxStandardDeviation);

            var selection = numbers.SelectPreferenceWithMaxDeviationThreshold(x => x, maxStandardDeviation);

            selection.Selection.Should().Be(numbers[2]);
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_throw_when_no_value_matches()
        {
            var numbers = new [] { 1.3, 1.2, 1.0, 0.9, 0.8, 0.8, 1.2 };
            var (mean, standardDeviation) = numbers.MeanStandardDeviation();
            var maxStandardDeviation = 0.01;

            numbers.Select(x => Math.Abs(x - mean)).All(d => d > standardDeviation * maxStandardDeviation)
                .Should().BeTrue();
            
            var selectionAction = new Action(() => numbers.SelectPreferenceWithMaxDeviationThreshold(x => x, maxStandardDeviation));

            selectionAction.Should().Throw<InvalidDataException>();
        }

        [Fact]
        public void ToCsvDistinctList_should_join_trimmed_lower_cased_ToString_results_with_spacing()
        {
            var strings = new[] {"ab ", "def", " klm", "KlM"};
            strings.ToCsvDistinctList(true).Should().Be("ab, def, klm");
        }
    }
}
