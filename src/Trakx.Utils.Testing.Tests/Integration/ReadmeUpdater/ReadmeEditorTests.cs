using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Polly;
using Trakx.Utils.Testing.ReadmeUpdater;
using Xunit;

namespace Trakx.Utils.Testing.Tests.Integration.ReadmeUpdater;

public class ReadmeEditorTests
{
    [Fact]
    public async Task ReadmeEditor_should_not_allow_concurrent_edits()
    {
        var file = Path.GetTempFileName();
        await using var writer = new StreamWriter(file);
        await writer.WriteAsync("hello ").ConfigureAwait(false);
        await writer.DisposeAsync().ConfigureAwait(false);

        var edits = Enumerable.Repeat("*", 10).Select(async s =>
        {
            using var editor = new ReadmeEditor(file);
            var content = await editor.ExtractReadmeContent().ConfigureAwait(false);
            await editor.UpdateReadmeContent(content + s).ConfigureAwait(false);
        }).ToArray();

        await Task.WhenAll(edits).ConfigureAwait(false);

        var delays = Enumerable.Repeat(TimeSpan.FromMilliseconds(100), 100);
        var retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(delays);
        var trials = await retryPolicy.ExecuteAndCaptureAsync(
                async () => await File.ReadAllTextAsync(file).ConfigureAwait(false))
            .ConfigureAwait(false);
        trials.Result.Should().Be("hello **********");
    }
}
