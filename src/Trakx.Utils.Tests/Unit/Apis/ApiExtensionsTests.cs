using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Apis
{
    public class ApiExtensionsTests
    {
        [Fact]
        public void Api_version_should_be_parsed_from_string()
        {
            var version = "0.1";
            var apiVersion = ApiVersion.Parse(version);
            apiVersion.MajorVersion.Should().Be(0);
            apiVersion.MinorVersion.Should().Be(1);
        }
    }
}
