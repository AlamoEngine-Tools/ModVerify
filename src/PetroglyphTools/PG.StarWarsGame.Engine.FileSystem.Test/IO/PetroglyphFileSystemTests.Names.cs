using System;
using System.IO;
using Xunit;
#if NETFRAMEWORK
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO;

public partial class PetroglyphFileSystemTests
{
    public static TheoryData<string, string> TestData_GetFileName => new()
    {
        { ".", "." },
        { "..", ".." },
        { "file", "file" },
        { "file.", "file." },
        { "file.exe", "file.exe" },
        { " . ", " . " },
        { " .. ", " .. " },
        { "fi le", "fi le" },
        { Path.Combine("baz", "file.exe"), "file.exe" },
        { Path.Combine("baz", "file.exe") + "/", "" },
        { Path.Combine("bar", "baz", "file.exe"), "file.exe" },
        { Path.Combine("bar", "baz", "file.exe") + "\\", "" },
        
        { "foo\\bar/file.exe", "file.exe" },
        { "foo/bar\\file.exe", "file.exe" },
    };
    
    [Theory, MemberData(nameof(TestData_GetFileName))]
    public void GetFileName_Span(string path, string expected)
    {
        PathAssert.Equal(expected.AsSpan(), _pgFileSystem.GetFileName(path.AsSpan()));
        Assert.Equal(expected, _pgFileSystem.GetFileName(path));
    }
    
    public static TheoryData<string, string> TestData_GetFileNameWithoutExtension => new()
    {
        { "", "" },
        { "file", "file" },
        { "file.exe", "file" },
        { "bar\\baz/file.exe", "file" },
        { "bar/baz\\file.exe", "file" },
        { Path.Combine("bar", "baz") + "\\", "" },
        { Path.Combine("bar", "baz") + "/", "" },
    };

    [Theory, MemberData(nameof(TestData_GetFileNameWithoutExtension))]
    public void GetFileNameWithoutExtension_Span(string path, string expected)
    {
        PathAssert.Equal(expected.AsSpan(), _pgFileSystem.GetFileNameWithoutExtension(path.AsSpan()));
        Assert.Equal(expected, _pgFileSystem.GetFileNameWithoutExtension(path));
#if Windows
        Assert.Equal(_pgFileSystem.GetFileName(path), _fileSystem.Path.GetFileName(path.AsSpan()));
#endif
    }
    
    [Theory,
     InlineData(null, null, null),
     InlineData(null, "exe", null),
     InlineData("", "", ""),
     InlineData("file.exe", null, "file"),
     InlineData("file.exe", "", "file."),
     InlineData("file", "exe", "file.exe"),
     InlineData("file", ".exe", "file.exe"),
     InlineData("file.txt", "exe", "file.exe"),
     InlineData("file.txt", ".exe", "file.exe"),
     InlineData("file.txt.bin", "exe", "file.txt.exe"),
     InlineData("dir/file.t", "exe", "dir/file.exe"),
     InlineData("dir\\file.t", "exe", "dir\\file.exe"),
     InlineData("dir/file.exe", "t", "dir/file.t"),
     InlineData("dir\\file.exe", "t", "dir\\file.t"),
     InlineData("dir/file", "exe", "dir/file.exe"),
     InlineData("dir\\file", "exe", "dir\\file.exe")]
    public void ChangeExtension(string? path, string? newExtension, string? expected)
    {
        Assert.Equal(expected, _pgFileSystem.ChangeExtension(path, newExtension));
        
#if Windows
        Assert.Equal(_pgFileSystem.ChangeExtension(path, newExtension), _fileSystem.Path.ChangeExtension(path, newExtension));
#endif
    }
}
