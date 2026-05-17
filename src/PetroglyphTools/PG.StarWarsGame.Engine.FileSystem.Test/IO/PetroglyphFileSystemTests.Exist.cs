using System;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO;

public partial class PetroglyphFileSystemTests
{
    [Theory]
#if Windows
    [InlineData("C:\\test.txt", true)]
    [InlineData("C:/test.txt", true)]
    [InlineData("D:\\path\\to\\file", true)]
    [InlineData("/test.txt", false)]
    [InlineData("\\test.txt", false)]
#else
    [InlineData("/test.txt", true)]
    [InlineData("/", true)]
    [InlineData("/a/b/c", true)]
    [InlineData("\\test.txt", false)]
    [InlineData("C:\\test.txt", false)]
#endif
    [InlineData("test.txt", false)]
    [InlineData("a/b/c", false)]
    [InlineData("./file.txt", false)]
    [InlineData("../file.txt", false)]
    [InlineData("", false)]
    public void IsPathFullyQualified_Exists(string path, bool expected)
    {
        Assert.Equal(expected, _pgFileSystem.IsPathFullyQualified_Exists(path.AsSpan()));
    }
}
