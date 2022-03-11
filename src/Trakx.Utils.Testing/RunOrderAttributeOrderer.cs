using System.Collections.Generic;
using System.Linq;
using Trakx.Utils.Testing.Attributes;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Trakx.Utils.Testing;

public class RunOrderAttributeOrderer : ITestCaseOrderer
{
    public const string AssemblyName = "Trakx.Utils.Testing";
    public const string TypeName = AssemblyName + "." + nameof(RunOrderAttributeOrderer);

    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(
        IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
    {
        string assemblyName = typeof(RunOrderAttribute).AssemblyQualifiedName!;
        var sortedMethods = new SortedDictionary<double, List<TTestCase>>();
        foreach (var testCase in testCases)
        {
            var priority = testCase.TestMethod.Method
                .GetCustomAttributes(assemblyName)
                .FirstOrDefault()
                ?.GetNamedArgument<double>(nameof(RunOrderAttribute.Order)) ?? 0.0;

            GetOrCreate(sortedMethods, priority).Add(testCase);
        }

        foreach (var testCase in
                 sortedMethods.Keys.SelectMany(
                     priority => sortedMethods[priority].OrderBy(
                         testCase => testCase.TestMethod.Method.Name)))
        {
            yield return testCase;
        }
    }

    private static TValue GetOrCreate<TKey, TValue>(
        IDictionary<TKey, TValue> dictionary, TKey key)
        where TKey : struct
        where TValue : new() =>
        dictionary.TryGetValue(key, out var result)
            ? result
            : dictionary[key] = new TValue();
}