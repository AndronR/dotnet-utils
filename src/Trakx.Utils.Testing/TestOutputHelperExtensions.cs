using System.Reflection;
using Xunit.Abstractions;

namespace Trakx.Utils.Testing;

public static class TestOutputHelperExtensions
{
    public static string GetCurrentTestName(this ITestOutputHelper output)
    {
        var currentTest = output
            .GetType()
            .GetField("test", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(output) as ITest;
        if (currentTest == null)
        {
            throw new ArgumentNullException(
                $"Failed to reflect current test as {nameof(ITest)} from {nameof(output)}");
        }

        var currentTestName = currentTest.TestCase.TestMethod.Method.Name;
        return currentTestName;
    }
}