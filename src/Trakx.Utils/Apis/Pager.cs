using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trakx.Utils.Apis;

public class Pager
{
    public int PageSize { get; }

    public int FirstPageIndex { get; }

    public Pager(int pageSize, int firstPageIndex = 1)
    {
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
        if (firstPageIndex < 0) throw new ArgumentOutOfRangeException(nameof(firstPageIndex));
        FirstPageIndex = firstPageIndex;
        PageSize = pageSize;
    }

    public async Task<List<T>> Enumerate<T>(Func<Page, Task<Response<List<T>>>> pagedQuery)
    {
        var page = new Page { MaxSize = PageSize, Number = FirstPageIndex };
        var allResults = new List<T>();
        var fetchedCount = 0;
        do
        {
            var results = (await pagedQuery.Invoke(page).ConfigureAwait(false)).Result;
            allResults.AddRange(results);
            fetchedCount = results.Count;
            page = page with { Number = page.Number + 1 };
        } while (fetchedCount == page.MaxSize);

        return allResults;
    }

    public async IAsyncEnumerable<T> EnumerateAsync<T>(Func<Page, Task<Response<List<T>>>> pagedQuery)
    {
        var page = new Page { MaxSize = PageSize, Number = FirstPageIndex };
        var fetchedCount = 0;
        do
        {
            var results = (await pagedQuery.Invoke(page).ConfigureAwait(false)).Result;
            foreach (var result in results) yield return result;
            fetchedCount = results.Count;
            page = page with { Number = page.Number + 1 };
        } while (fetchedCount == page.MaxSize);
    }
}

public readonly record struct Page
{
    public int Number { get; init; }
    public int MaxSize { get; init; }
}
