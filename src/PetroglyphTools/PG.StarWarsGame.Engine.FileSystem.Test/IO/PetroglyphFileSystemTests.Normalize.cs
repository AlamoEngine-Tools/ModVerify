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
}
