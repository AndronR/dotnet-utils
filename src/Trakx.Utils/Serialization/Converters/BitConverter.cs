namespace Trakx.Utils.Serialization.Converters;

public static class BitConverter
{
    public static byte[] GetBytes(this decimal dec)
    {
        var bytes = decimal.GetBits(dec)
            .SelectMany(System.BitConverter.GetBytes)
            .ToArray();
        return bytes;
    }
    public static decimal ToDecimal(this byte[] bytes)
    {
        if (bytes.Length != 16) throw new ArgumentOutOfRangeException(nameof(bytes), "A decimal must be created from exactly 16 bytes");
        var integers = new int[4];
        for (var i = 0; i <= 15; i += 4)
        {
            integers[i / 4] = System.BitConverter.ToInt32(bytes, i);
        }
        return new decimal(integers);
    }
}