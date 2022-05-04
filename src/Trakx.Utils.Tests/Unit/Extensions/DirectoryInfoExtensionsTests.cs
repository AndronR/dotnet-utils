using FluentAssertions;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions;

public class DirectoryInfoExtensionsTests
{
    [Fact]
    public void TryWalkBackToRepositoryRoot_should_find_repository_root_if_it_exists()
    {
        var info = new DirectoryInfo(Environment.CurrentDirectory);
        var found = info.TryWalkBackToRepositoryRoot(out var rootDirectory);
        found.Should().BeTrue();
        rootDirectory.Should().NotBeNull();
        rootDirectory!.EnumerateDirectories().Select(d => d.Name).Should().Contain(new [] {".git", "src"});
        rootDirectory.EnumerateFiles().Select(d => d.Name).Should().Contain(new [] { ".gitignore", "README.md"});
    }

    [Fact]
    public void TryWalkBackToRepositoryRoot_should__not_find_repository_root_if_it_does_not_exist()
    {
        var info = new DirectoryInfo(Path.GetTempPath());
        var found = info.TryWalkBackToRepositoryRoot(out var rootDirectory);
        found.Should().BeFalse();
        rootDirectory.Should().BeNull();
    }


    [Fact]
    public void GetDefaultEnvFilePath_should_return_null_if_not_in_a_repository_folder()
    {
        var info = new DirectoryInfo(Path.GetTempPath());
        var envFilePath = info.GetDefaultEnvFilePath();
        envFilePath.Should().BeNull();
    }

    [Fact]
    public void GetDefaultEnvFilePath_should_not_be_null_if_in_a_repo()
    {
        var envFilePath = default(DirectoryInfo).GetDefaultEnvFilePath();
        envFilePath.Should().NotBeNullOrEmpty();
        envFilePath.Should().EndWith(Path.Combine("src", ".env"));
    }
}