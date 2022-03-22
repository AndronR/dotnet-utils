using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Trakx.Utils.Attributes;
using Trakx.Utils.Testing.ReadmeUpdater;
using Xunit;
using Xunit.Abstractions;

namespace Trakx.Utils.Testing.Tests.Integration;

internal class FakeConfiguration
{
    [AwsParameter]
    [SecretEnvironmentVariable("SecretAbc")]
    public string? SecretString { get; set; }

    [AwsParameter("Forced/Path")]
    [SecretEnvironmentVariable("Secret123")]
    public int? SecretNumber { get; set; }

    [AwsParameter]
    [SecretEnvironmentVariable]
    public string? ImplicitlyNamedSecret { get; set; }

#pragma warning disable S1144, IDE0051 // Unused private types or members should be removed
    // ReSharper disable once UnusedMember.Local
    private string? NotSecret { get; set; }
#pragma warning restore S1144, IDE0051 // Unused private types or members should be removed

}

internal class ReadmeDocumentationUpdater : ReadmeDocumentationUpdaterBase
{
    public ReadmeDocumentationUpdater(ITestOutputHelper output, IReadmeEditor? editor = null, bool simulateExistingValidFile = true)
        : base(output, editor ?? Substitute.For<IReadmeEditor>())
    {
        if(!simulateExistingValidFile) return;
        var fakeReadmeContent = "```secretsEnvVariables" + Environment.NewLine +
                                "FakeConfiguration__ImplicitlyNamedSecret=********" + Environment.NewLine +
                                "Secret123=********" + Environment.NewLine +
                                "SecretAbc=********" + Environment.NewLine +
                                "SomeConfigClassName__SomePropertyName=********" + Environment.NewLine +
                                "```" + Environment.NewLine +
                                Environment.NewLine +
                                "```awsParams" + Environment.NewLine +
                                "FakeConfiguration__ImplicitlyNamedSecret=********" + Environment.NewLine +
                                "Secret123=********" + Environment.NewLine +
                                "SecretAbc=********" + Environment.NewLine +
                                "SomeConfigClassName__SomePropertyName=********" + Environment.NewLine +
                                "```" + Environment.NewLine
                                ;

        Editor.ExtractReadmeContent().Returns(fakeReadmeContent);

        Editor.When(e => e.UpdateReadmeContent(Arg.Any<string>()))
            .Do(ci => (ci[0] as string).Should().Be(fakeReadmeContent, "the content should not change."));
    }
}

public class ReadmeDocumentationUpdaterTests
{
    private readonly ReadmeDocumentationUpdaterBase _updater;
    private readonly IReadmeEditor _readmeEditor;

    public ReadmeDocumentationUpdaterTests(ITestOutputHelper output)
    {
        _readmeEditor = Substitute.For<IReadmeEditor>();
        _updater = new ReadmeDocumentationUpdater(output, _readmeEditor, false);
    }

    [Fact]
    public async Task UpdateEnvFileDocumentation_should_not_update_when_section_does_not_exist()
    {
        _readmeEditor.ExtractReadmeContent().ReturnsForAnyArgs(
            "## Existing Section" + Environment.NewLine +
            "with a paragraph, and some text" + Environment.NewLine);

        var envSuccess = await _updater.UpdateDocumentation(new EnvVarDocumentationUpdater());
        envSuccess.Should().BeFalse();

        var awsSuccess = await _updater.UpdateDocumentation(new AwsDocumentationUpdater("Test/Assembly"));
        awsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateDocumentation_for_env_variables_should_update_when_section_exist()
    {
        var secretFromOtherAssembly = "Secret__FromOtherAssembly=********" + Environment.NewLine;
        var existingSecret = "FakeConfiguration__SecretAbc=********" + Environment.NewLine +
                             secretFromOtherAssembly +
                             "SomeConfigClassName__SomePropertyName=********";
        var secretsToBeAdded = new List<string>
        {
            "FakeConfiguration__ImplicitlyNamedSecret=********",
            "Secret123=********" + Environment.NewLine,
        };

        var textToKeep =
            "## Existing Section" + Environment.NewLine +
            "with a paragraph, and some text" + Environment.NewLine +
            Environment.NewLine + Environment.NewLine +
            "## Creating your local .env file" + Environment.NewLine +
            "In order to be able to run some integration tests, you should create a `.env` file in the `src` folder with the following variables:" + Environment.NewLine +
            "```secretsEnvVariables" + Environment.NewLine;
        var readmeContent =
            textToKeep +
            existingSecret + Environment.NewLine +
            "```" + Environment.NewLine;

        _readmeEditor.ExtractReadmeContent().ReturnsForAnyArgs(
            readmeContent);

        var success = await _updater.UpdateDocumentation(new EnvVarDocumentationUpdater());
        success.Should().BeTrue();

        await _readmeEditor.Received(1).UpdateReadmeContent(Arg.Any<string>()).ConfigureAwait(false);
        var firstArgument = _readmeEditor.ReceivedCalls()
            .Single(c => c.GetMethodInfo().Name == nameof(_readmeEditor.UpdateReadmeContent)).GetArguments()[0] as string;
        secretsToBeAdded.ForEach(s => firstArgument.Should().Contain(s));
        firstArgument.Should().Contain(secretFromOtherAssembly);
    }

    [Fact]
    public async Task UpdateDocumentation_for_aws_should_update_when_section_exist()
    {
        var secretFromOtherAssembly = "blablabla/Secret/FromOtherAssembly" + Environment.NewLine;
        var existingSecret = "/Some/Assembly/FakeConfiguration/SecretString" + Environment.NewLine +
                             secretFromOtherAssembly +
                             "/Hello/SomeConfigClassName/SomePropertyName";
        var secretsToBeAdded = new List<string>
        {
            "/Some/Assembly/FakeConfiguration/ImplicitlyNamedSecret" + Environment.NewLine,
            "/Some/Assembly/Forced/Path" + Environment.NewLine,
        };

        var textToKeep =
            "## Existing Section" + Environment.NewLine +
            "with a paragraph, and some text" + Environment.NewLine +
            Environment.NewLine + Environment.NewLine +
            "## AWS Parameters" + Environment.NewLine +
            "In order to be able to run some integration tests you should ensure that you have access to the following AWS parameters :" + Environment.NewLine +
            "```awsParams" + Environment.NewLine;
        var readmeContent =
            textToKeep +
            existingSecret + Environment.NewLine +
            "```" + Environment.NewLine;

        _readmeEditor.ExtractReadmeContent().ReturnsForAnyArgs(
            readmeContent);

        var variablesPrefix = "/Some/Assembly/";
        var success = await _updater.UpdateDocumentation(new AwsDocumentationUpdater(variablesPrefix));
        success.Should().BeTrue();

        await _readmeEditor.Received(1).UpdateReadmeContent(Arg.Any<string>()).ConfigureAwait(false);
        var firstArgument = _readmeEditor.ReceivedCalls()
            .Single(c => c.GetMethodInfo().Name == nameof(_readmeEditor.UpdateReadmeContent)).GetArguments()[0] as string;
        secretsToBeAdded.ForEach(s => firstArgument.Should().Contain(s));
        firstArgument.Should().Contain(secretFromOtherAssembly);
    }
}
