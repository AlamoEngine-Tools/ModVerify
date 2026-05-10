using System.IO;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Utilities;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO;

public partial class PetroglyphFileSystemTests
{
    [Theory]
#if Windows
    [InlineData("dir\\file.txt", "dir\\file.txt")]
    [InlineData("dir/file.txt", "dir\\file.txt")]
    [InlineData("\\dir\\subdir\\", "\\dir\\subdir\\")]
    [InlineData("/dir\\subdir/", "\\dir\\subdir\\")]
#else
    [InlineData("dir\\file.txt", "dir/file.txt")]
    [InlineData("dir/file.txt", "dir/file.txt")]
    [InlineData("\\dir\\subdir\\", "/dir/subdir/")]
    [InlineData("/dir\\subdir/", "/dir/subdir/")]
#endif
    public void NormalizePath(string path, string expected)
    {
        var vsb = new ValueStringBuilder();
        try
        {
            vsb.Append(path);
            _pgFileSystem.NormalizePath(ref vsb);

            Assert.Equal(expected, vsb.ToString());
        }
        finally
        {
            vsb.Dispose();
        }
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("/", "/")]
    [InlineData("/foo", "/foo")]
    [InlineData("/foo/bar", "/foo/bar")]
    [InlineData("/a/b/c.xml", "/a/b/c.xml")]
    [InlineData("/Mods/Test/Data/Xml/foo.xml", "/Mods/Test/Data/Xml/foo.xml")]
    [InlineData("/.", "/")]
    [InlineData("/./foo", "/foo")]
    [InlineData("/foo/.", "/foo")]
    [InlineData("/foo/./bar", "/foo/bar")]
    [InlineData("/foo/././bar", "/foo/bar")]
    [InlineData("/./foo/./bar/.", "/foo/bar")]
    [InlineData("/foo/..", "/")]
    [InlineData("/foo/../bar", "/bar")]
    [InlineData("/foo/bar/..", "/foo")]
    [InlineData("/foo/bar/../baz", "/foo/baz")]
    [InlineData("/a/b/c/../../d", "/a/d")]
    [InlineData("/..", "/")]
    [InlineData("/../foo", "/foo")]
    [InlineData("/foo/../..", "/")]
    [InlineData("/foo/../../bar", "/bar")]
    [InlineData("/a/./b/../c", "/a/c")]
    [InlineData("/a/b/./../c", "/a/c")]
    [InlineData("/Other/../Mods/./Test", "/Mods/Test")]
    [InlineData("/foo//bar", "/foo/bar")]
    [InlineData("/a///b", "/a/b")]
    [InlineData("//foo", "/foo")]
    [InlineData("/foo/", "/foo")]
    [InlineData("/foo/bar/", "/foo/bar")]
    [InlineData("/foo/bar//", "/foo/bar")]
    [InlineData("/...", "/...")]
    [InlineData("/.foo", "/.foo")]
    [InlineData("/foo/...", "/foo/...")]
    [InlineData("/foo/..bar", "/foo/..bar")]
    [InlineData("/foo/.bar/baz", "/foo/.bar/baz")]
    public void NormalizeDotSegmentsInPlace_RewritesBufferCorrectly(string input, string expected)
    {
        // The function operates on host-native separators; test data uses '/' for readability
        // and is converted at entry.
        input = input.Replace('/', Path.DirectorySeparatorChar);
        expected = expected.Replace('/', Path.DirectorySeparatorChar);

        var vsb = new ValueStringBuilder();
        try
        {
            vsb.Append(input);
            _pgFileSystem.NormalizeDotSegmentsInPlace(ref vsb);

            Assert.Equal(expected, vsb.ToString());
        }
        finally
        {
            vsb.Dispose();
        }
    }
}
