using System;

namespace Trakx.Utils.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SecretEnvironmentVariableAttribute : Attribute
{
    public string? VarName { get; }

    public SecretEnvironmentVariableAttribute(string? varName = null)
    {
        VarName = varName;
    }

    public SecretEnvironmentVariableAttribute(string configurationTypeName, string propertyName)
    {
        VarName = $"{configurationTypeName}__{propertyName}";
    }
}