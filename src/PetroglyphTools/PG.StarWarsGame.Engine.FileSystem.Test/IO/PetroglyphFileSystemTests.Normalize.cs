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
        vsb.Append(path);
        _pgFileSystem.NormalizePath(ref vsb);

        Assert.Equal(expected, vsb.ToString());
    }

    [Theory]
    // Empty input — early return.
    [InlineData("", "")]
    // Root only — no segments to process.
    [InlineData("/", "/")]
    // No dot segments — passthrough (just rewritten to itself in place).
    [InlineData("/foo", "/foo")]
    [InlineData("/foo/bar", "/foo/bar")]
    [InlineData("/a/b/c.xml", "/a/b/c.xml")]
    [InlineData("/Mods/Test/Data/Xml/foo.xml", "/Mods/Test/Data/Xml/foo.xml")]
    // Single dot segments are dropped.
    [InlineData("/.", "/")]
    [InlineData("/./foo", "/foo")]
    [InlineData("/foo/.", "/foo")]
    [InlineData("/foo/./bar", "/foo/bar")]
    [InlineData("/foo/././bar", "/foo/bar")]
    [InlineData("/./foo/./bar/.", "/foo/bar")]
    // ".." pops the previous segment, including the slash before it.
    [InlineData("/foo/..", "/")]
    [InlineData("/foo/../bar", "/bar")]
    [InlineData("/foo/bar/..", "/foo")]
    [InlineData("/foo/bar/../baz", "/foo/baz")]
    [InlineData("/a/b/c/../../d", "/a/d")]
    // ".." clamps at root — never produces a path outside the rooted hierarchy.
    [InlineData("/..", "/")]
    [InlineData("/../foo", "/foo")]
    [InlineData("/foo/../..", "/")]
    [InlineData("/foo/../../bar", "/bar")]
    // Mixed "." and ".." in one path.
    [InlineData("/a/./b/../c", "/a/c")]
    [InlineData("/a/b/./../c", "/a/c")]
    [InlineData("/Other/../Mods/./Test", "/Mods/Test")]
    // Runs of separators are collapsed to a single '/'.
    [InlineData("/foo//bar", "/foo/bar")]
    [InlineData("/a///b", "/a/b")]
    [InlineData("//foo", "/foo")]
    // A trailing separator is dropped.
    [InlineData("/foo/", "/foo")]
    [InlineData("/foo/bar/", "/foo/bar")]
    [InlineData("/foo/bar//", "/foo/bar")]
    // Segments that start or end with dots but aren't "." / ".." are preserved verbatim.
    [InlineData("/...", "/...")]
    [InlineData("/.foo", "/.foo")]
    [InlineData("/foo/...", "/foo/...")]
    [InlineData("/foo/..bar", "/foo/..bar")]
    [InlineData("/foo/.bar/baz", "/foo/.bar/baz")]
    public void NormalizeDotSegmentsInPlace_RewritesBufferCorrectly(string input, string expected)
    {
        var vsb = new ValueStringBuilder();
        vsb.Append(input);
        PetroglyphFileSystem.NormalizeDotSegmentsInPlace(ref vsb);

        Assert.Equal(expected, vsb.ToString());
    }
}
