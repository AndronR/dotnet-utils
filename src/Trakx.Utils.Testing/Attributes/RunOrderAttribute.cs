using System;

namespace Trakx.Utils.Testing.Attributes;
//https://docs.microsoft.com/en-us/dotnet/core/testing/order-unit-tests?pivots=xunit

[AttributeUsage(AttributeTargets.Method)]
public class RunOrderAttribute : Attribute
{
    /// <summary>
    /// The lower the value of <see cref="Order"/>, the earlier the test
    /// will be placed in the queue.
    /// </summary>
    public double Order { get; }

    public RunOrderAttribute(double order) => Order = order;
}