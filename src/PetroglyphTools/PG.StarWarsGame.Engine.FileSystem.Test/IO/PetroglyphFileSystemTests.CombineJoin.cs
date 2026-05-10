using System;
using System.IO;
using PG.StarWarsGame.Engine.Utilities;
using Xunit;
#if NETFRAMEWORK
using AnakinRaW.CommonUtilities.FileSystem;
#endif

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
        try
        {
            _pgFileSystem.JoinPath(path1.AsSpan(), path2.AsSpan(), ref vsb);
            var result = vsb.ToString();
            Assert.Equal(expected, result);
#if Windows
            Assert.Equal(result, _fileSystem.Path.Join(path1, path2));
#endif
        }
        finally
        {
            vsb.Dispose();
        }
    }

    [Fact]
    public void CombinePath_FirstArgNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _pgFileSystem.CombinePath(null!, "b"));
    }

    [Fact]
    public void CombinePath_SecondArgNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _pgFileSystem.CombinePath("a", null!));
    }

    [Fact]
    public void JoinPath_BothEmpty_LeavesBufferUntouched()
    {
        var vsb = new ValueStringBuilder();
        try
        {
            vsb.Append("preexisting");
            _pgFileSystem.JoinPath(ReadOnlySpan<char>.Empty, ReadOnlySpan<char>.Empty, ref vsb);
            Assert.Equal("preexisting", vsb.ToString());
        }
        finally
        {
            vsb.Dispose();
        }
    }
}
