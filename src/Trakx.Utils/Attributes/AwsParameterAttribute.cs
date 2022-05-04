namespace Trakx.Utils.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AwsParameterAttribute : ConfigurationParameterAttribute
{
    protected override string GetConfigPath(string className, string propertyName) => Path ?? $"{className}/{propertyName}";

    /// <summary>
    /// A custom path fragment for the AWS parameter. If none is provided
    /// the path will be {ClassName}/{VariableName}
    /// </summary>
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public string? Path { get; }

    public AwsParameterAttribute(string? path = null)
    {
        Path = path;
    }
}
