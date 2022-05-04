using System.Numerics;

namespace Trakx.Utils.Extensions;

public static class NumberExtensions
{
    public static BigInteger AsAPowerOf10(this ushort number) => BigInteger.Pow(10, number);
    public static decimal AsAPowerOf10(this int number) => (decimal)Math.Pow(10, number);

    /// <summary>
    /// Takes a quantity as expressed in set protocol, in the number of units native to the
    /// components and scales it as a number of units native to the indice to which it belongs.
    /// </summary>
    /// <param name="unscaledQuantity">Quantity in number units native to the component.</param>
    /// <param name="componentDecimals">Number of decimals supported by by the ERC20 contract of the component.</param>
    /// <param name="indiceNaturalUnit">Natural unit of the indice, see set protocol for more details. Generally 10
    /// as it is the convention for rebalancing indices.</param>
    /// <returns>Quantity in number units native to the indice.</returns>
    /// <remarks>18 is the number of decimals of the contracts produced by the set factory.</remarks>
    public static decimal ScaleConstituentQuantity(this decimal unscaledQuantity, ushort componentDecimals,
        ushort indiceNaturalUnit) => unscaledQuantity * (18 - componentDecimals - indiceNaturalUnit).AsAPowerOf10();

    /// <summary>
    /// Takes a quantity expressed as a number of units native to the indice to which it belongs, and returns it
    /// as expressed in set protocol, in the number of units native to the components and scales it.
    /// </summary>
    /// <param name="scaledQuantity">Quantity in number units native to the indice.</param>
    /// <param name="componentDecimals">Number of decimals supported by by the ERC20 contract of the component.</param>
    /// <param name="indiceNaturalUnit">Natural unit of the indice, see set protocol for more details. Generally 10
    /// as it is the convention for rebalancing indices.</param>
    /// <returns>Quantity in number units native to the component.</returns>
    /// <remarks>18 is the number of decimals of the contracts produced by the set factory.</remarks>
    public static decimal DescaleConstituentQuantity(this decimal scaledQuantity, ushort componentDecimals,
        ushort indiceNaturalUnit) => scaledQuantity * (componentDecimals + indiceNaturalUnit - 18).AsAPowerOf10();

    /// <summary>
    /// It looks like .Net Standard doesn't have the good RoundingDirection available, so we do it manually.
    /// </summary>
    /// <param name="value">Value we want to floor.</param>
    /// <param name="precision">Number of decimals after which we want to truncate.</param>
    public static decimal FloorWithPrecision(this decimal value, int precision)
    {
        var precisionMultiplier = precision.AsAPowerOf10();
        return Math.Floor(precisionMultiplier * value) / precisionMultiplier;
    }
}