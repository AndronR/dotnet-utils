using FluentAssertions;
using System.Text.RegularExpressions;
using Trakx.Utils.Extensions;
using Trakx.Utils.Testing.Attributes;
using Trakx.Utils.Testing.ReadmeUpdater;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable S2699

namespace Trakx.Utils.Testing;

[TestCaseOrderer(RunOrderAttributeOrderer.TypeName, RunOrderAttributeOrderer.AssemblyName)]
public abstract class OpenApiGeneratedCodeModifier
{
    private readonly ITestOutputHelper _output;
    protected List<string> FilePaths { get; } = new ();

    private static readonly string ClassDefinitionRegex = @"(?<before>[\s]{4}\[System\.CodeDom\.Compiler\.GeneratedCode\(""NJsonSchema"", [^\]]+\]\r?\n[\s]{4}public partial )(?<class>class)(?<after> [\w:\s]+\s?\r?\n)";
    private static readonly string SetPropertyRegex = @"(?<before>[\s]{8}\[Newtonsoft\.Json\.JsonProperty\([^\]]+\]\r?\n[\s]{8}public [^\s]+ [^\s]+ \{ get; )(?<set>set)";
    private static readonly string ClientRegex = @"public partial interface I(?<client>[^\s]+)";

    protected OpenApiGeneratedCodeModifier(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact, RunOrder(1)]
    public async Task Replace_model_class_by_records()
    {
        foreach (var filePath in FilePaths)
        {
            var content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            content = Regex.Replace(content, ClassDefinitionRegex, "${before}" + "record" + "${after}");
            content = Regex.Replace(content, SetPropertyRegex, "${before}" + "init");
            await File.WriteAllTextAsync(filePath, content);
        }
    }

    [Fact, RunOrder(2)]
    public async Task Remove_warnings()
    {
        foreach (var filePath in FilePaths)
        {
            var content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            var existingWarningStart = @"#pragma warning disable 108";
            var warningStartToAdd = @"#pragma warning disable CS0618";
            if (!content.Contains(warningStartToAdd)) content = content.Replace(existingWarningStart,
                warningStartToAdd + Environment.NewLine + existingWarningStart);
            var existingWarningStop = @"#pragma warning restore 108";
            var warningStopToAdd = @"#pragma warning restore CS0618";
            if (!content.Contains(warningStopToAdd)) content = content.Replace(existingWarningStop,
                existingWarningStop + Environment.NewLine + warningStopToAdd);
            await File.WriteAllTextAsync(filePath, content);
        }
    }


    [Fact, RunOrder(3)]
    public async Task Output_client_names()
    {
        foreach (var filePath in FilePaths)
        {
            var content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            var clientRegex = new Regex(ClientRegex);
            var clients = clientRegex.Matches(content).Select(m => m.Groups["client"].Value)
                .OrderBy(s => s);
            _output.WriteLine(string.Join(", ", clients.Select(c => $"\"{c}\"")));
        }
    }


    [Fact, RunOrder(4)]
    public async Task Do_not_call_send_methods_asynchronously()
    {
        foreach (var filePath in FilePaths)
        {
            var content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            content = content.Replace(@"var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);",
                @"var response_ = client_.Send(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken);");
            await File.WriteAllTextAsync(filePath, content);
        }
    }

    private DirectoryInfo GetRepositoryRootDirectory()
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);
        if (!directory.TryWalkBackToRepositoryRoot(out var repositoryRoot))
            repositoryRoot.Should().NotBeNull("Tests should be running from its repository root.");
        _output.WriteLine($"using {repositoryRoot!.FullName} as repository root path.");
        return repositoryRoot;
    }

    [Fact, RunOrder(5)]
    public async Task Update_Readme()
    {
        var readmeDirectoryInfo = GetRepositoryRootDirectory();
        var readmeFilePath = Path.Combine(readmeDirectoryInfo.FullName, "README.md");
        using var readmeEditor = new ReadmeEditor(readmeFilePath);
        var content = await readmeEditor.ExtractReadmeContent();
        if (!content.Contains("How to regenerate C# API clients"))
            content += @"
## How to regenerate C# API clients

* If you work with external API, you probably need to update OpenAPI definition added to the project. It's usually openApi3.yaml file.
* Do right click on the project and select Edit Project File. In the file change value of `GenerateApiClient` property to true.
* Rebuild the project. `NSwag` target will be executed as post action.
* The last thing to be done is to run integration test `OpenApiGeneratedCodeModifier` that will rewrite auto generated C# classes to use C# 9 features like records and init keyword.
";
        await readmeEditor.UpdateReadmeContent(content);
    }
}

#pragma warning restore S2699
