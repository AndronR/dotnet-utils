using System.Globalization;
using System.Text;
using FluentAssertions;
using Trakx.Utils.Serialization.Converters;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions;

public class BitConverterTests
{
    [Theory]
    [InlineData("123.456")]
    [InlineData("0.9999999999999999999999999999")]
    public void BitConverter_should_convert_to_and_from_decimal_without_loss(string decimalAsString)
    {
        var value = decimal.Parse(decimalAsString);
        var bytes = value.GetBytes();
        var rebuiltValue = bytes.ToDecimal();

        rebuiltValue.ToString(CultureInfo.InvariantCulture).Should().Be(decimalAsString);
    }

    [Theory]
    [InlineData("123456789012345")] //too short
    [InlineData("12345678901234567")] //too long
    public void BitConverter_should_throw_IndexOutOfRange_on_invalid_input_size(string bytesSeed)
    {
        var bytes = Encoding.UTF8.GetBytes(bytesSeed);
        bytes.Length.Should().NotBe(16);
        new Action(() => bytes.ToDecimal()).Should().Throw<ArgumentOutOfRangeException>();
    }
}