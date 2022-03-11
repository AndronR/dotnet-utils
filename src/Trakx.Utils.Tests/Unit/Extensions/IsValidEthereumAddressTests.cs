using FluentAssertions;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions;

public class IsValidEthereumAddressTests
{
    [Fact]
    public void IsValidEthereumAddressTests_should_only_validate_ehtereum_addresses()
    {
        "0x43cE8afa6985C86485640c7FEC81bc8FDd66E95f".IsValidEthereumAddress().Should().BeTrue();
        "0x43cE8afa6985C86485640c7FEC81bc8FDd66E9f".IsValidEthereumAddress().Should().BeFalse();
        "0x43cE8afa6985C86485640c7FEC81bc8FDd66E95f.".IsValidEthereumAddress().Should().BeFalse();
    }
}