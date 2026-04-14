using System;
using System.Collections.Generic;
using System.IO;
using AnakinRaW.CommonUtilities.FileSystem;
using Xunit;

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
    
    public static IEnumerable<object[]> TestData_GetFileName_Volume()
    {
        yield return [":", ":"];
        yield return [".:", ".:"];
        yield return [".:.", ".:."];     // Not a valid drive letter
        yield return ["file:", "file:"];
        yield return [":file", ":file"];
        yield return ["file:exe", "file:exe"];
        yield return ["baz\\file:exe", "file:exe"];
        yield return ["bar/baz/file:exe", "file:exe"];
    }
    
    [Theory, MemberData(nameof(TestData_GetFileName_Volume))]
    public void GetFileName_Volume(string path, string expected)
    {
        // We used to break on ':' on Windows. This is a valid file name character for alternate data streams.
        // Additionally, the character can show up on unix volumes mounted to Windows.
#if !NETFRAMEWORK
        Assert.Equal(expected, Path.GetFileName(path));
        Assert.Equal(expected, _pgFileSystem.GetFileName(path));
#if Windows
        Assert.Equal(_pgFileSystem.GetFileName(path), _fileSystem.Path.GetFileName(path.AsSpan()));
#endif
#endif

        PathAssert.Equal(expected.AsSpan(), _pgFileSystem.GetFileName(path.AsSpan()));
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
