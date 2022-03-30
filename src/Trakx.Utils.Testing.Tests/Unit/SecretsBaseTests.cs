using FluentAssertions;
using Xunit;
using static System.Environment;

namespace Trakx.Utils.Testing.Tests.Unit;

public class SecretsBaseTests
{
    [Fact]
    public void GetConfigurationFromEnv_should_not_set_property_value_if_environment_variables_are_not_known()
    {
        SetEnvironmentVariable($"{nameof(TestConfigurationSection)}__{nameof(TestConfigurationSection.PublicKey)}", "coucou");
        SetEnvironmentVariable($"{nameof(TestConfigurationSection)}__{nameof(TestConfigurationSection.PrivateKey)}", "hello");

        var config = ConfigurationHelper.GetConfigurationFromEnv<TestConfigurationSection>();
        config.PublicKey.Should().Be("coucou");
        config.PrivateKey.Should().Be("hello");
        config.NotSetByEnv.Should().BeNull();
    }

    private record TestConfigurationSection
    {
        #nullable disable
        public string PrivateKey { get; init; }
        public string PublicKey { get; init; }
        #nullable restore
        public string? NotSetByEnv { get; init; }

    }
}
