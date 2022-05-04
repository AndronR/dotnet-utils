using System.Text;
using System.Text.RegularExpressions;
using Trakx.Utils.Attributes;

namespace Trakx.Utils.Testing.ReadmeUpdater;

public interface IDocumentationUpdater<T> where T : ConfigurationParameterAttribute
{
    Regex SectionToUpdateRegex { get; }
    string ParametersPrefix { get; }
    string ParametersSuffix { get; }
    void AppendSection(StringBuilder builder, List<string> expectedParams);

}