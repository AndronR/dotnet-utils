using System;
using FluentAssertions;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions;

public class NumberExtensionsTests
{
    [Fact]
    public void ToThePowerOf10_should_elevate_numbers_correctly()
    {
        2.AsAPowerOf10().Should().Be(100);
        (-3).AsAPowerOf10().Should().Be(0.001m);
        new Action(() => 50.AsAPowerOf10()).Should().Throw<OverflowException>();
    }

    [Fact]
    public void ScaleConstituentQuantity_should_work()
    {
        5_000_000m.ScaleConstituentQuantity(15, 10).Should().Be(0.5m);
        999_000_000m.ScaleConstituentQuantity(18, 10).Should().Be(.0999m);
        999_000_000m.ScaleConstituentQuantity(18, 10).Should()
            .NotBe(999_000_000m.ScaleConstituentQuantity(17, 10));
        999_000_000m.ScaleConstituentQuantity(18, 10).Should()
            .NotBe(999_000_000m.ScaleConstituentQuantity(18, 11));
    }

    [Fact]
    public void UnScaleConstituentQuantity_should_work()
    {
        0.5m.DescaleConstituentQuantity(15, 11).Should().Be(50_000_000m);
        99.9m.DescaleConstituentQuantity(3, 10).Should().Be(0.000999m);
        999_000_000m.DescaleConstituentQuantity(18, 10).Should()
            .NotBe(999_000_000m.ScaleConstituentQuantity(17, 10));
        999_000_000m.DescaleConstituentQuantity(18, 10).Should()
            .NotBe(999_000_000m.ScaleConstituentQuantity(18, 11));
    }

    [Fact]
    public void Scaling_and_descaling_should_be_identity()
    {
        12345.6789m.ScaleConstituentQuantity(13, 11).DescaleConstituentQuantity(13, 11)
            .Should().Be(12345.6789m);
    }
}