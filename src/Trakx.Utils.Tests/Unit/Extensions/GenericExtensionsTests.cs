using System.Collections.Generic;
using FluentAssertions;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions;

public class GenericExtensionsTests
{
    [Fact]
    public void AsSingletonList_should_return_item_in_list()
    {
        "hello".AsSingletonList().Should()
            .BeAssignableTo<List<string>>()
            .And.BeEquivalentTo("hello");
    }

    [Fact]
    public void AsSingletonArray_should_return_item_in_array()
    {
        "hello".AsSingletonArray().Should()
            .BeAssignableTo<string[]>()
            .And.BeEquivalentTo("hello");
    }

    [Fact]
    public void AsSingletonIEnumerable_should_return_item_in_IEnumerable()
    {
        "hello".AsSingletonIEnumerable().Should()
            .BeAssignableTo<IEnumerable<string>>()
            .And.BeEquivalentTo("hello");
    }
}