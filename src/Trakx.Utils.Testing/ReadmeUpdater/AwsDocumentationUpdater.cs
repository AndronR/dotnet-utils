using System.Text;
using System.Text.RegularExpressions;
using Trakx.Utils.Attributes;

namespace Trakx.Utils.Testing.ReadmeUpdater;

public class AwsDocumentationUpdater : IDocumentationUpdater<AwsParameterAttribute>
{
    public AwsDocumentationUpdater(string parametersPrefix)
    {
        ParametersPrefix = parametersPrefix;
    }

    public Regex SectionToUpdateRegex { get; } = new (@"```awsParams\r?\n(?<params>(?<params>([\w\/\-]+)\r?\n)+)```\r?\n");
    public string ParametersPrefix { get; }
    public string ParametersSuffix => "";

    public void AppendSection(StringBuilder builder, List<string> expectedParams)
    {
        builder.AppendLine("## AWS Parameters");
        builder.AppendLine("In order to be able to run some integration tests you should ensure that you have access to the following AWS parameters :");
        builder.AppendLine("```awsParams");
        builder.AppendLine(string.Join(Environment.NewLine, expectedParams));
        builder.AppendLine("```");
        builder.AppendLine(string.Empty);
    }
}
