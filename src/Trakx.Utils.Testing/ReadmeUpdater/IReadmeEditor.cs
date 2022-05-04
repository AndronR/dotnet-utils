using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Trakx.Utils.Testing.ReadmeUpdater;

internal interface IReadmeEditor : IDisposable
{
    Task<string> ExtractReadmeContent();
    Task UpdateReadmeContent(string newContent);
}