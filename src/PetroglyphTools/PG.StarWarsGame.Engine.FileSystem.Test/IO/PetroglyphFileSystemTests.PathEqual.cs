using AnakinRaW.CommonUtilities.FileSystem;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO;

public partial class PetroglyphFileSystemTests
{
    [Theory]
    [InlineData("dir/file.txt", "DIR\\FILE.TXT", true)]
    [InlineData("dir/file.txt", "dir/other.txt", false)]
    [InlineData("a/b/c", "a\\b\\c", true)]
    public void PathsAreEqual(string pathA, string pathB, bool expected)
    {
        Assert.Equal(expected, _pgFileSystem.PathsAreEqual(pathA, pathB));
#if Windows
        Assert.Equal(_pgFileSystem.PathsAreEqual(pathA, pathB), _fileSystem.Path.AreEqual(pathA, pathB));
#endif
    }
}
