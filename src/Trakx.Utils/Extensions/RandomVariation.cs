using System;

namespace Trakx.Utils.Extensions;

public static class RandomVariation
{
    private static readonly Random Random = new(DateTime.Now.Millisecond);

    /// <summary>
    /// A convenience method to add a random variation to a given value.
    /// </summary>
    /// <param name="original">The original value around which we want a variation.</param>
    /// <param name="maxPercentageVariation">The maximal amplitude of the variation.</param>
    /// <param name="random">Supply your own instance of Random if predictable result is needed.</param>
    /// <returns></returns>
    public static decimal AddRandomVariation(this decimal original, decimal maxPercentageVariation, Random? random = default)
    {
        var variation = (decimal)(2  * ((random ?? Random).NextDouble() - 0.5));
        var randomMove = original * maxPercentageVariation * variation;
        return original + randomMove;
    }
}