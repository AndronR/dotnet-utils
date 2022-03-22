using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Trakx.Utils.Apis;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Apis;

public class PagerTests
{
    private readonly IEnumerable<int> _expectedList;
    private readonly IResponseClient _client;

    public interface IResponseClient
    {
        Task<Response<List<int>>> GetResponse(int page, int limit);
    }

    public PagerTests()
    {
        _expectedList = Enumerable.Range(0, 100);
        _client = Substitute.For<IResponseClient>();

        _client.GetResponse(Arg.Any<int>(), Arg.Any<int>())
            .Returns(ci => new Response<List<int>>(200, null,
                _expectedList.Skip(((int)ci[0] - 1)*(int)ci[1]).Take((int)ci[1]).ToList()));
    }

    [Fact]
    public async Task Enumerate_should_go_through_all_records()
    {
        var pager = new Pager(30);
        var list = await pager.Enumerate(page => _client.GetResponse(page.Number, page.MaxSize));
        list.Should().BeEquivalentTo(_expectedList);
        await _client.Received(4).GetResponse(Arg.Any<int>(), Arg.Any<int>());
    }


    [Fact]
    public async Task EnumerateAsync_should_go_through_all_records()
    {
        var pager = new Pager(35);
        var list = await pager.EnumerateAsync(page => _client.GetResponse(page.Number, page.MaxSize)).ToListAsync();
        list.Should().BeEquivalentTo(_expectedList);
        await _client.Received(3).GetResponse(Arg.Any<int>(), Arg.Any<int>());
    }
}
