using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions;

public class RandomVariationTest
{
    [Fact]
    public void RandomVariation_should_modify_original_value_within_specified_range()
    {
        var original = 100m;
        var maxVariation = 0.2m;
        var variationResults = new List<decimal>();
            
        for (var i = 0; i < 1000; i++)
        {
            variationResults.Add(original.AddRandomVariation(maxVariation));
        }

        variationResults.All(d => 80m <= d && d <= 120m).Should().BeTrue();
        variationResults.Count(d => d == 100).Should().BeLessThan(10);
    }
}