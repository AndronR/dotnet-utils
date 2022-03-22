using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Trakx.Utils.Attributes;

namespace Trakx.Utils.Testing.ReadmeUpdater;

public class EnvVarDocumentationUpdater : IDocumentationUpdater<SecretEnvironmentVariableAttribute>
{
    public Regex SectionToUpdateRegex { get; } = new(@"```secretsEnvVariables\r?\n(?<params>(?<params>([\w]+)=(\*)+\r?\n)+)```\r?\n");
    public string ParametersPrefix { get; } = string.Empty;

    public string ParametersSuffix { get; } = "=********";
    public void AppendSection(StringBuilder builder, List<string> expectedParams)
    {
        builder.AppendLine("## Creating your local .env file");
        builder.AppendLine("In order to be able to run some integration tests, you should create a `.env` file in the `src` folder with the following variables:");
        builder.AppendLine("```secretsEnvVariables");
        builder.AppendLine(string.Join(Environment.NewLine, expectedParams));
        builder.AppendLine("```");
        builder.AppendLine(string.Empty);
    }
}
