using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Trakx.Utils.Attributes;
using Trakx.Utils.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Trakx.Utils.Testing;

/// <summary>
/// This class should be inherited in the est suites of projects which need
/// secrets to be provided by environment variables. It will run trigger the run
/// of <see cref="TryUpdateEnvFileDocumentation_should_create_env_template_file"/>
/// that updates .env file template.
/// </summary>
public abstract class EnvFileDocumentationUpdaterBase
{
    private readonly ITestOutputHelper _output;
    private readonly IReadmeEditor _editor;
    internal IReadmeEditor Editor => _editor;
    private static readonly Regex DotEnvSection = new(@"```secretsEnvVariables\r?\n(?<envVars>(?<envVar>([\w]+)=(\*)+\r?\n)+)```\r?\n");
    private readonly PathAssemblyResolver _resolver;
    protected readonly Assembly ImplementingAssembly;

    protected EnvFileDocumentationUpdaterBase(ITestOutputHelper output) : this(output, default) { }

#pragma warning disable S3442 // "abstract" classes should not have "public" constructors
    internal EnvFileDocumentationUpdaterBase(ITestOutputHelper output, IReadmeEditor? editor)
#pragma warning restore S3442 // "abstract" classes should not have "public" constructors
    {
        _output = output;
        var readmeDirectoryInfo = GetRepositoryRootDirectory();
        var readmeFilePath = Path.Combine(readmeDirectoryInfo.FullName, "README.md");

        _editor = editor ?? new ReadmeEditor(readmeFilePath);
        _resolver = GetTrakxAssemblyResolver();
        ImplementingAssembly = GetType().Assembly;
    }

    [Fact]
    public async Task TryUpdateEnvFileDocumentation_should_create_env_template_file()
    {
        var envFileDocCreated = await UpdateEnvFileDocumentation().ConfigureAwait(false);
        envFileDocCreated.Should().BeTrue();
    }

    public async Task<bool> UpdateEnvFileDocumentation()
    {
        try
        {
            var expectedEnvVarSecrets = GetExpectedEnvVarSecretsFromLoadedAssemblies();
            if (!expectedEnvVarSecrets.Any()) return true;

            var readmeContent = await _editor.ExtractReadmeContent();
            var secretsMentionedInReadme = DotEnvSection.Match(readmeContent);

            if (!secretsMentionedInReadme.Success)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Your README.md file should contain the following section:");
                AppendExampleEnvFileDocumentationSection(stringBuilder, expectedEnvVarSecrets);
                _output.WriteLine(stringBuilder.ToString());
                return false;
            }

            var contentToReplace = secretsMentionedInReadme.Groups["envVars"].Value;
            var knownSecrets = secretsMentionedInReadme.Groups["envVars"].Value.Split(Environment.NewLine).Where(s => !string.IsNullOrWhiteSpace(s));
            var allSecrets = GetExpectedEnvVarSecretsFromLoadedAssemblies().Union(knownSecrets).Distinct().OrderBy(s => s);
            var newContent = string.Join(Environment.NewLine, allSecrets) + Environment.NewLine;
            var newReadmeContent = readmeContent.Replace(contentToReplace, newContent, StringComparison.InvariantCulture);

            await _editor.UpdateReadmeContent(newReadmeContent).ConfigureAwait(false);

            return true;
        }
        catch (Exception e)
        {
            _output.WriteLine("Failed to update env file documentation.");
            _output.WriteLine(e.ToString());
            return false;
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

    private static void AppendExampleEnvFileDocumentationSection(StringBuilder builder, List<string> expectedEnvVars)
    {
        builder.AppendLine("## Creating your local .env file");
        builder.AppendLine("In order to be able to run some integration tests, you should create a `.env` file in the `src` folder with the following variables:");
        builder.AppendLine("```secretsEnvVariables");
        builder.AppendLine(string.Join(Environment.NewLine, expectedEnvVars));
        builder.AppendLine("```");
        builder.AppendLine(string.Empty);
    }

    private List<string> GetExpectedEnvVarSecretsFromLoadedAssemblies()
    {
        var assemblies = LoadReferencedAssembliesMetadata()
            .Where(a => a.FullName?.StartsWith("Trakx.") ?? false);
        var explorationContext = new AssemblyLoadContext(nameof(EnvFileDocumentationUpdaterBase), true);

        var result = assemblies.SelectMany(currentAssembly =>
            {
                _output.WriteLine("Inspecting assembly {0}", currentAssembly.FullName);
                var assemblyTypes = currentAssembly.GetTypes();
                if (!assemblyTypes.Any(t => t.FullName?.EndsWith("Configuration") ?? false))
                    return Enumerable.Empty<string>();
                var fullyLoadedAssembly = explorationContext.LoadFromAssemblyPath(currentAssembly.Location);
                var configTypes = fullyLoadedAssembly.GetTypes().Where(t => t.FullName?.EndsWith("Configuration") ?? false);
                var secrets = configTypes
                    .SelectMany(t => t.GetProperties()
                        .Select(p =>
                            p.GetCustomAttribute(typeof(SecretEnvironmentVariableAttribute)) is SecretEnvironmentVariableAttribute attribute
                                ? (attribute.VarName ?? $"{t.Name}__{p.Name}")+"=********"
                                : null))
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
                return secrets.Select(s => s!);
            })
            .Distinct()
            .OrderBy(abc => abc)
            .ToList();

        explorationContext.Unload();

        return result;
    }

    private List<Assembly> LoadReferencedAssembliesMetadata(int maxRecursions = 10)
    {
        var explorationContext = new MetadataLoadContext(_resolver);

        var knownAssemblies = explorationContext.GetAssemblies().Union(new[] { ImplementingAssembly }).ToList();
        var recursions = 0;

        List<string> newAssemblyNames;
        do
        {
            recursions++;

            var knownNames = knownAssemblies.Select(a => a.GetName().FullName).Distinct().ToList();
            var referencedNames = knownAssemblies.SelectMany(
                    a => a.GetReferencedAssemblies().Where(n => n.FullName.StartsWith("Trakx")))
                .Select(a => a.FullName);
            newAssemblyNames = referencedNames.Except(knownNames).ToList();
            foreach (var name in newAssemblyNames)
            {
                try
                {
                    knownAssemblies.Add(explorationContext.LoadFromAssemblyName(name));
                }
                catch (Exception e) { _output.WriteLine("Failed to load assembly {0} with exception {1}", name, e); }
            }

        } while (newAssemblyNames.Any() && recursions < maxRecursions);

        return knownAssemblies;
    }

    private static PathAssemblyResolver GetTrakxAssemblyResolver()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var executingAssemblyLocation = new FileInfo(executingAssembly.Location).Directory!.GetFiles("Trakx.*.dll")
            .Select(f => f.FullName).ToList();
        var runtimeAssembliesLocations =
            Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

        var assemblyPaths = runtimeAssembliesLocations.Union(executingAssemblyLocation);
        var resolver = new PathAssemblyResolver(assemblyPaths);
        return resolver;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        _editor.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
