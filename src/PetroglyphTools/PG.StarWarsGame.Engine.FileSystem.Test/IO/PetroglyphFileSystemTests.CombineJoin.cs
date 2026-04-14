using System;
using System.IO;
using AnakinRaW.CommonUtilities.FileSystem;
using PG.StarWarsGame.Engine.Utilities;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO;

public partial class PetroglyphFileSystemTests
{
    [Theory]
#if Windows
    [InlineData("a", "b", "a\\b")]
    [InlineData("a/", "b", "a/b")]
    [InlineData("a\\", "b", "a\\b")]
    [InlineData("", "b", "b")]
    [InlineData("a", "", "a")]
    [InlineData("/", "b", "/b")]
    [InlineData("a", "/b", "/b")]
    [InlineData("a", "\\b", "\\b")]
    [InlineData("a/b", "c/d", "a/b\\c/d")]
    [InlineData("a\\b", "c\\d", "a\\b\\c\\d")]
    [InlineData("a/b/", "c/d", "a/b/c/d")]
    [InlineData("a\\b\\", "c\\d", "a\\b\\c\\d")]
#else
    [InlineData("a", "b", "a/b")]
    [InlineData("a/", "b", "a/b")]
    [InlineData("a\\", "b", "a\\b")]
    [InlineData("", "b", "b")]
    [InlineData("a", "", "a")]
    [InlineData("/", "b", "/b")]
    [InlineData("a", "/b", "/b")]
    [InlineData("a", "\\b", "\\b")]
    [InlineData("a/b", "c/d", "a/b/c/d")]
    [InlineData("a\\b", "c\\d", "a\\b/c\\d")]
    [InlineData("a/b/", "c/d", "a/b/c/d")]
    [InlineData("a\\b\\", "c\\d", "a\\b\\c\\d")]
#endif
    public void CombinePath(string pathA, string pathB, string expected)
    {
        var result = _pgFileSystem.CombinePath(pathA, pathB);
        Assert.Equal(expected, result);
#if Windows
        Assert.Equal(Path.Combine(pathA, pathB), result);
#endif
    }

    [Theory]
#if Windows
    [InlineData("a", "b", "a\\b")]
    [InlineData("a/", "b", "a/b")]
    [InlineData("a\\", "b", "a\\b")]
    [InlineData("", "b", "b")]
    [InlineData("a", "", "a")]
    [InlineData("/", "b", "/b")]
    [InlineData("a", "/b", "a/b")]
    [InlineData("a", "\\b", "a\\b")]
    [InlineData("a/b", "c/d", "a/b\\c/d")]
    [InlineData("a\\b", "c\\d", "a\\b\\c\\d")]
#else
    [InlineData("a", "b", "a/b")]
    [InlineData("a/", "b", "a/b")]
    [InlineData("a\\", "b", "a\\b")]
    [InlineData("", "b", "b")]
    [InlineData("a", "", "a")]
    [InlineData("/", "b", "/b")]
    [InlineData("a", "/b", "a/b")]
    [InlineData("a", "\\b", "a\\b")]
    [InlineData("a/b", "c/d", "a/b/c/d")]
    [InlineData("a\\b", "c\\d", "a\\b/c\\d")]
#endif
    public void JoinPath(string path1, string path2, string expected)
    {
        var vsb = new ValueStringBuilder();
        _pgFileSystem.JoinPath(path1.AsSpan(), path2.AsSpan(), ref vsb);
        var result = vsb.ToString();
        Assert.Equal(expected, result);
#if Windows
        Assert.Equal(result, _fileSystem.Path.Join(path1, path2));
#endif
    }
}
