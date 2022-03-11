using System.IO;
using System.Linq;
using System.Reflection;

namespace Trakx.Utils.Extensions;

public static class DirectoryInfoExtensions
{
    public static bool TryWalkBackToRepositoryRoot(this DirectoryInfo? directory, out DirectoryInfo? repositoryRootDirectory)
    {
        repositoryRootDirectory = directory ?? new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
        do
        {
            if (repositoryRootDirectory.IsGitRepositoryRoot()) return true;
            repositoryRootDirectory = repositoryRootDirectory.Parent;
        } while (repositoryRootDirectory != null);

        return false;
    }

    public static bool IsGitRepositoryRoot(this DirectoryInfo? directory)
    {
        return directory != null && directory.Exists &&
               directory.GetDirectories(".git").Any(d => d.Name == ".git") &&
               directory.GetDirectories("src").Any(d => d.Name == "src") &&
               directory.GetFiles("README.md").Any(d => d.Name == "README.md") &&
               directory.GetFiles(".gitignore").Any(d => d.Name == ".gitignore");
    }

    public static string? GetDefaultEnvFilePath(this DirectoryInfo? startingDirectory)
    {
        var directoryInfo = startingDirectory ?? new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
        return directoryInfo.TryWalkBackToRepositoryRoot(out var rootDirectory)
            ? Path.Combine(rootDirectory!.FullName, "src", ".env")
            : null;
    }
}
