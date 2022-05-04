using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace Trakx.Utils.Testing;

public class MockCreator
{
    public readonly Random Random;
    protected static readonly string HexChars = "abcdef01234566789";
    protected static readonly string Alphabet = "abcdefghijklmnopqrstuvwxyz";
    public readonly string TestName;

    public MockCreator(ITestOutputHelper output)
    {
        TestName = output.GetCurrentTestName();
        Random = new Random(TestName.GetHashCode());
    }

    public string GetRandomAddressEthereum() => "0x" + new string(Enumerable.Range(0, 40)
        .Select(_ => HexChars[Random.Next(0, HexChars.Length)]).ToArray());
    public string GetRandomEthereumTransactionHash() => "0x" + new string(Enumerable.Range(0, 64)
        .Select(_ => HexChars[Random.Next(0, HexChars.Length)]).ToArray());

    public string GetRandomString(int size) => new (Enumerable.Range(0, size)
        .Select(_ => Alphabet[Random.Next(0, Alphabet.Length)]).ToArray());

    public string GetRandomYearMonthSuffix() => $"{Random.Next(20, 36):00}{Random.Next(1, 13):00}";

    public DateTime GetRandomUtcDateTime()
    {
        var dateTime = GetRandomUtcDateTimeOffset();
        return dateTime.UtcDateTime;
    }

    public ulong GetRandomUnscaledAmount() => (ulong)Random.Next(1, int.MaxValue);
    public ushort GetRandomDecimals() => (ushort)Random.Next(0, 19);

    public DateTimeOffset GetRandomUtcDateTimeOffset()
    {
        var firstJan2020 = new DateTimeOffset(2020, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var firstJan2050 = new DateTimeOffset(2050, 01, 01, 0, 0, 0, 0, TimeSpan.Zero);
        var timeBetween2020And2050 = firstJan2050.Subtract(firstJan2020);

        var randomDay = firstJan2020 + TimeSpan.FromDays(Random.Next(0, (int)timeBetween2020And2050.TotalDays));
        return randomDay;
    }

    public decimal GetRandomPrice() => Random.Next(1, int.MaxValue) / 1e5m;
    public decimal GetRandomValue() => Random.Next(1, int.MaxValue) / 1e2m;
    public TimeSpan GetRandomTimeSpan(double maxDurationInDays = 1000) => TimeSpan.FromSeconds(Random.Next(1, (int)TimeSpan.FromDays(maxDurationInDays).TotalSeconds));

    public T GetRandomEnumValue<T>()
    {
        var length = typeof(T).GetEnumValues().Length;
        return (T)typeof(T).GetEnumValues().GetValue(Random.Next(0, length))!;
    }

    public string GetEmailAddress(string? domain = null)
    {
        const string regex = @"^[a-zA-Z0-9][a-zA-Z0-9-]{1,61}[a-zA-Z0-9]\.[a-zA-Z]{2,}$";
        if (!string.IsNullOrWhiteSpace(domain) && !Regex.IsMatch(domain, regex))
            throw new ArgumentException($"{domain} is not a valid domain name");
        var emailAddress = $"{GetRandomString(10)}@{domain ?? GetRandomString(5) + "." + GetRandomString(3)}";
        return emailAddress;
    }

}
