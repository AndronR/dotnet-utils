using FluentAssertions;
using Trakx.Utils.Attributes;
using Trakx.Utils.Testing.Attributes;
using Xunit;
using static System.Environment;

namespace Trakx.Utils.Testing.Tests.Unit;

[TestCaseOrderer(RunOrderAttributeOrderer.TypeName, RunOrderAttributeOrderer.AssemblyName)]
public class SecretsBaseTests
{
    [Fact, RunOrder(2)]
    public void SecretBase_should_set_property_value_from_known_environment_variables()
    {
        SetEnvironmentVariable("env_var_name", "coucou");
        SetEnvironmentVariable($"{nameof(TestSecrets)}__{nameof(TestSecrets.ImplicitlyNamed)}", "hello");
        SetEnvironmentVariable("SomeConfigClassName__SomePropertyName", "ola");

        var secrets = new TestSecrets();

        secrets.EnvironmentVar.Should().Be("coucou");
        secrets.ImplicitlyNamed.Should().Be("hello");
        secrets.ExplicitTypePropertyNamedSecret.Should().Be("ola");
    }

    [Fact, RunOrder(1)]
    public void SecretBase_should_not_set_property_value_if_environment_variables_are_not_known()
    {
        var secrets = new TestSecrets();

        secrets.EnvironmentVar.Should().BeNull();
    }

    private record TestSecrets : SecretsBase
    {
        [SecretEnvironmentVariable("env_var_name")]
#pragma warning disable CS8618
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string EnvironmentVar { get; init; }

        [SecretEnvironmentVariable]
        public string? ImplicitlyNamed { get; init; }

        [SecretEnvironmentVariable("SomeConfigClassName", "SomePropertyName")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string? ExplicitTypePropertyNamedSecret { get; init; }
#pragma  warning restore CS8618
    }
}
