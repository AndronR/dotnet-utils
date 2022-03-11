using FluentAssertions;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions;

public class LevenshteinDistanceTests
{
    [Theory]
    [InlineData("hello", "hellow", 1)]
    [InlineData("hello", "chellow", 2)]
    [InlineData("hello", "heldo", 1)]
    public void CalculateLevenshteinDistance_should_increase_with_dissimilarities(string source, string target, decimal expectedScore)
    {
        source.CalculateLevenshteinDistance(target).Should().Be(expectedScore);
    }

    [Theory]
    [InlineData("hello", "heLlo", 0.5)]
    [InlineData("hello", "heLLo", 1)]
    [InlineData("hello", "hewLLo", 2)]
    [InlineData("hello", "heWLLo", 2)]
    public void CalculateLevenshteinDistance_should_increase_less_for_cas_mismatch(string source, string target, decimal expectedScore)
    {
        source.CalculateLevenshteinDistance(target).Should().Be(expectedScore);
    }

    [Fact]
    public void FindBestLevenshteinMatch_should_return_best_match()
    {
        var hellos = new[] {"heLLo", "hewLLo", "heLlo", "heWLLo"};
        var match = "hello".FindBestLevenshteinMatch(hellos);
        match.Should().NotBeNull();
        match.Should().Be("heLlo");
    }

    [Fact]
    public void FindBestLevenshteinMatch_should_return_null_if_match_not_close_enough()
    {
        var hellos = new[] { "heLLoMate", "hewLLoDude", "heLloMyFriend", "heWLLoYou" };
        var match = "hello".FindBestLevenshteinMatch(hellos, 1);
        match.Should().BeNull();
    }
}