using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trakx.Utils.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace Trakx.Utils.Startup.Tests.Integration;

public class AddAwsSystemsManagerConfigurationTests
{
    private readonly ITestOutputHelper _output;

    public AddAwsSystemsManagerConfigurationTests(ITestOutputHelper output)
    {
        _output = output;
    }
    private record FakeConfiguration
    {
        [AwsParameterAttribute]
#nullable disable
        public string SecretConfigParam { get; init; }
#nullable enable
    }

    const string AspNetCoreEnvironmentVariable = "ASPNETCORE_ENVIRONMENT";
    const string Environment = "ci-cd";

    [Fact(Skip = "look at https://github.com/aws-actions/configure-aws-credentials/issues/271 and try to fix the permissions of the ci-cd-service-user")]
    //[Fact]
    public void AddAwsSystemsManagerConfiguration_should_use_path_from_namespace()
    {
        var envVarSetup = $"{AspNetCoreEnvironmentVariable}={Environment}";
        DotNetEnv.Env.LoadContents(envVarSetup);
        System.Environment.GetEnvironmentVariable(AspNetCoreEnvironmentVariable)!.Should().Be(Environment);

        var hostBuilder = Host.CreateDefaultBuilder();

        hostBuilder.AddAwsSystemsManagerConfiguration<FakeConfiguration>();

        hostBuilder.ConfigureServices((builderContext, services) =>
        {
            services.AddSingleton(builderContext.Configuration);
        });
        var host = hostBuilder.Build();

        var config = host.Services.GetRequiredService<IConfiguration>();

        //the definition and values of these parameters is set on AWS parameter store
        config.GetSection(nameof(FakeConfiguration))[nameof(FakeConfiguration.SecretConfigParam)].Should().Be("Three Views of a Secret");

        var configRoot = config as IConfigurationRoot;
        _output.WriteLine(configRoot.GetDebugView());
    }
}
