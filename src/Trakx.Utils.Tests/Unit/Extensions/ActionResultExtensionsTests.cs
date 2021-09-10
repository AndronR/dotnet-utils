using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions
{
    public class ActionResultExtensionsTests
    {
        [Fact]
        public void GetResult_from_action_result()
        {
            const string value = "hello world";
            var actionResult = new ActionResult<string>(new OkObjectResult(value));
            actionResult.GetResult().Should().Be(value);
        }
    }
}