using System;

namespace Trakx.Utils.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SecretEnvironmentVariableAttribute : ConfigurationParameterAttribute
{
    public SecretEnvironmentVariableAttribute() { }

    [Obsolete("error prone, use the empty constructor and let the readme updater do its job.")]
    public SecretEnvironmentVariableAttribute(string className, string propertyName)
    {
        VarName = GetConfigPath(className, propertyName);
    }

    public string? VarName { get; }

    public SecretEnvironmentVariableAttribute(string? varName = null)
    {
        VarName = varName;
    }

    protected sealed override string GetConfigPath(string className, string propertyName)
        => VarName ?? $"{className}__{propertyName}";
}
