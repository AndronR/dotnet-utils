using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using FluentAssertions;
using Trakx.Utils.Attributes;
using Trakx.Utils.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Trakx.Utils.Testing.ReadmeUpdater;

/// <summary>
/// This class should be inherited in the est suites of projects which need
/// secrets to be provided by environment variables. It will run trigger the run
/// of <see cref="TryUpdateReadmeDocumentation_should_update_env_section"/>
/// that updates .env file template.
/// </summary>
public abstract class ReadmeDocumentationUpdaterBase : IDisposable
{
    private readonly int _maxRecursions;
    private readonly ITestOutputHelper _output;
    private readonly IReadmeEditor _editor;
    internal IReadmeEditor Editor => _editor;
    private readonly PathAssemblyResolver _resolver;
    protected readonly Assembly ImplementingAssembly;

    protected ReadmeDocumentationUpdaterBase(ITestOutputHelper output, int maxRecursions = 1) : this(output, default, maxRecursions)
    { }

#pragma warning disable S3442 // "abstract" classes should not have "public" constructors
    internal ReadmeDocumentationUpdaterBase(ITestOutputHelper output, IReadmeEditor? editor, int maxRecursions = 1)
#pragma warning restore S3442 // "abstract" classes should not have "public" constructors
    {
        _output = output;
        var readmeDirectoryInfo = GetRepositoryRootDirectory();
        var readmeFilePath = Path.Combine(readmeDirectoryInfo.FullName, "README.md");

        _editor = editor ?? new ReadmeEditor(readmeFilePath);
        _resolver = GetTrakxAssemblyResolver();
        _maxRecursions = maxRecursions;

        ImplementingAssembly = GetType().Assembly;
    }

    [Fact]
    public async Task TryUpdateReadmeDocumentation_should_update_env_section()
    {
        var envFileDocCreated = await UpdateDocumentation(new EnvVarDocumentationUpdater()).ConfigureAwait(false);
        envFileDocCreated.Should().BeTrue();
    }

    [Fact]
    public async Task TryUpdateReadmeDocumentation_should_update_aws_section()
    {
        var assemblyName = ImplementingAssembly.GetName().Name!;

        var variablesPrefix = "/" + (assemblyName.EndsWith(".Tests")
                ? assemblyName.Remove(assemblyName.Length - ".Tests".Length, ".Tests".Length)
                : assemblyName).Replace(".", "/") + "/";
        var updater = new AwsDocumentationUpdater(variablesPrefix);
        var awsDocCreated = await UpdateDocumentation(updater).ConfigureAwait(false);
        awsDocCreated.Should().BeTrue();
    }

    public async Task<bool> UpdateDocumentation<T>(IDocumentationUpdater<T> updater)
        where T : ConfigurationParameterAttribute
    {
        try
        {
            var expectedEnvVarSecrets = GetExpectedEnvVarSecretsFromLoadedAssemblies<T>(updater.ParametersPrefix, updater.ParametersSuffix);
            if (!expectedEnvVarSecrets.Any()) return true;

            var readmeContent = await _editor.ExtractReadmeContent();
            var secretsMentionedInReadme = updater.SectionToUpdateRegex.Match(readmeContent);

            if (!secretsMentionedInReadme.Success)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Your README.md file should contain the following section:");
                updater.AppendSection(stringBuilder, expectedEnvVarSecrets);
                _output.WriteLine(stringBuilder.ToString());
                return false;
            }

            var contentToReplace = secretsMentionedInReadme.Groups["params"].Value;
            var knownSecrets = secretsMentionedInReadme.Groups["params"].Value.Split(Environment.NewLine).Where(s => !string.IsNullOrWhiteSpace(s));
            var allSecrets = expectedEnvVarSecrets.Union(knownSecrets).Distinct().OrderBy(s => s);
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

    private List<string> GetExpectedEnvVarSecretsFromLoadedAssemblies<T>(string prefix = "", string postfix = "") where T : ConfigurationParameterAttribute
    {
        var assemblies = LoadReferencedAssembliesMetadata()
            .Where(a => a.FullName?.StartsWith("Trakx.") ?? false);
        var explorationContext = new AssemblyLoadContext(nameof(ReadmeDocumentationUpdaterBase), true);

        var result = assemblies.SelectMany(currentAssembly =>
            {
                _output.WriteLine("Inspecting assembly {0}", currentAssembly.FullName);
                var configTypes = GetAllConfigurationTypesFromAssembly(currentAssembly, explorationContext);
                var secrets = ConfigurationParameterAttribute.GetConfigPathsFromConfigTypes<T>(configTypes, prefix, postfix);
                return secrets.Select(s => s!);
            })
            .Distinct()
            .OrderBy(abc => abc)
            .ToList();

        explorationContext.Unload();

        return result;
    }

    private static List<Type> GetAllConfigurationTypesFromAssembly(Assembly currentAssembly,
        AssemblyLoadContext explorationContext)
    {
        var assemblyTypes = currentAssembly.GetTypes();
        if (!assemblyTypes.Any(t => t.FullName?.EndsWith("Configuration") ?? false))
        {
            return new List<Type>();
        }

        var fullyLoadedAssembly = explorationContext.LoadFromAssemblyPath(currentAssembly.Location);
        var configTypes = fullyLoadedAssembly.GetTypes().Where(t => t.FullName?.EndsWith("Configuration") ?? false)
            .ToList();
        return configTypes;
    }

    private List<Assembly> LoadReferencedAssembliesMetadata()
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

        } while (newAssemblyNames.Any() && recursions <= _maxRecursions);

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
