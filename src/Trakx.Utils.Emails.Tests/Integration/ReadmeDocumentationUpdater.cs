using Trakx.Utils.Testing.ReadmeUpdater;
using Xunit.Abstractions;

namespace Trakx.Utils.Emails.Tests.Integration;

public class ReadmeDocumentationUpdater : ReadmeDocumentationUpdaterBase
{
    public ReadmeDocumentationUpdater(ITestOutputHelper output, int maxRecursions = 1) : base(output, maxRecursions)
    {

    }
}
