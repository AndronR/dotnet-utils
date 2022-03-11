using System.IO;
using System.Linq;
using System.Reflection;
using DotNetEnv;
using Trakx.Utils.Attributes;
using Trakx.Utils.Extensions;
using static System.Environment;
 
namespace Trakx.Utils.Testing;

/// <summary>
/// Inherit from this class to create a class which can initialise its
/// SecretEnvironmentVariable decorated properties with environment variables.
/// </summary>
public abstract record SecretsBase
{
    protected SecretsBase()
    {
        var envFilePath = DirectoryInfoExtensions.GetDefaultEnvFilePath(null);
        if (envFilePath != null) Env.Load(Path.Combine(envFilePath));

        var typeProperties = GetType()
            .GetProperties()
            .ToList();

        foreach (var property in typeProperties)
        {
            if (property.GetCustomAttribute(typeof(SecretEnvironmentVariableAttribute)) is SecretEnvironmentVariableAttribute attribute)
            {
                property.SetValue(this, GetEnvironmentVariable(attribute.VarName ?? $"{GetType().Name}__{property.Name}"));
            }
        }
    }
}