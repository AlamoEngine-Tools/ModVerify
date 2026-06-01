using System;
using System.IO;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Utilities;
using Xunit;
#if NETFRAMEWORK
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace PG.StarWarsGame.Engine.FileSystem.Test.IO;

public partial class PetroglyphFileSystemTests
{
    private static string Sep = PetroglyphFileSystem.DirectorySeparatorChar.ToString();
    private static string AltSep = PetroglyphFileSystem.AltDirectorySeparatorChar.ToString();
    private static string OsSep = Path.DirectorySeparatorChar.ToString();

    // Mirrors TestData_JoinTwoPaths, but with CombinePath semantics: a rooted second path replaces the
    // first entirely. Null cases are omitted because CombinePath throws on null (see the dedicated facts).
    public static TheoryData<string, string, string> TestData_CombineTwoPaths = new()
    {
        { "", "", "" },
        { Sep, "", Sep },
        { AltSep, "", AltSep },
        { "", Sep, Sep },
        { "", AltSep, AltSep },
        { Sep, Sep, Sep },
        { AltSep, AltSep, AltSep },
        { "a", "", "a" },
        { "", "a", "a" },
        { "a", "a", $"a{OsSep}a" },
        { $"a{Sep}", "a", $"a{Sep}a" },
        { "a", $"{Sep}a", $"{Sep}a" },
        { $"a{Sep}", $"{Sep}a", $"{Sep}a" },
        { "a", $"a{Sep}", $"a{OsSep}a{Sep}" },
        { $"a{AltSep}", "a", $"a{AltSep}a" },
        { "a", $"{AltSep}a", $"{AltSep}a" },
        { $"a{Sep}", $"{AltSep}a", $"{AltSep}a" },
        { $"a{AltSep}", $"{AltSep}a", $"{AltSep}a" },
        { "a", $"a{AltSep}", $"a{OsSep}a{AltSep}" }
    };

    [Theory]
    [MemberData(nameof(TestData_CombineTwoPaths))]
    public void CombinePath(string pathA, string pathB, string expected)
    {
        var result = _pgFileSystem.CombinePath(pathA, pathB);
        Assert.Equal(expected, result);
#if Windows
        Assert.Equal(Path.Combine(pathA, pathB), result);
#endif
    }

    public static TheoryData<string?, string?, string> TestData_JoinTwoPaths = new()
    {
        { "", "", "" },
        { Sep, "", Sep },
        { AltSep, "", AltSep },
        { "", Sep, Sep },
        { "", AltSep, AltSep },
        { Sep, Sep, $"{Sep}{Sep}" },
        { AltSep, AltSep, $"{AltSep}{AltSep}" },
        { "a", "", "a" },
        { "", "a", "a" },
        { "a", "a", $"a{OsSep}a" },
        { $"a{Sep}", "a", $"a{Sep}a" },
        { "a", $"{Sep}a", $"a{Sep}a" },
        { $"a{Sep}", $"{Sep}a", $"a{Sep}{Sep}a" },
        { "a", $"a{Sep}", $"a{OsSep}a{Sep}" },
        { $"a{AltSep}", "a", $"a{AltSep}a" },
        { "a", $"{AltSep}a", $"a{AltSep}a" },
        { $"a{Sep}", $"{AltSep}a", $"a{Sep}{AltSep}a" },
        { $"a{AltSep}", $"{AltSep}a", $"a{AltSep}{AltSep}a" },
        { "a", $"a{AltSep}", $"a{OsSep}a{AltSep}" },
        { null, null, ""},
        { null, "a", "a"},
        { "a", null, "a"}
    };

    [Theory]
    [MemberData(nameof(TestData_JoinTwoPaths))]
    public void JoinPath(string? path1, string? path2, string expected)
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

    public static TheoryData<string?, string?, string?, string> TestData_JoinThreePaths = new()
    {
        { "", "", "", "" },
        { Sep, Sep, Sep, $"{Sep}{Sep}{Sep}" },
        { AltSep, AltSep, AltSep, $"{AltSep}{AltSep}{AltSep}" },
        { "a", "", "", "a" },
        { "", "a", "", "a" },
        { "", "", "a", "a" },
        { "a", "", "a", $"a{OsSep}a" },
        { "a", "a", "", $"a{OsSep}a" },
        { "", "a", "a", $"a{OsSep}a" },
        { "a", "a", "a", $"a{OsSep}a{OsSep}a" },
        { "a", Sep, "a", $"a{Sep}a" },
        { $"a{Sep}", "", "a", $"a{Sep}a" },
        { $"a{Sep}", "a", "", $"a{Sep}a" },
        { "", $"a{Sep}", "a", $"a{Sep}a" },
        { "a", "", $"{Sep}a", $"a{Sep}a" },
        { $"a{AltSep}", "", "a", $"a{AltSep}a" },
        { $"a{AltSep}", "a", "", $"a{AltSep}a" },
        { "", $"a{AltSep}", "a", $"a{AltSep}a" },
        { "a", "", $"{AltSep}a", $"a{AltSep}a" },
        { null, null, null, "" },
        { "a", null, null, "a" },
        { null, "a", null, "a" },
        { null, null, "a", "a" },
        { "a", null, "a", $"a{OsSep}a" }
    };

    [Theory]
    [MemberData(nameof(TestData_JoinThreePaths))]
    public void JoinPath_ThreePaths(string? path1, string? path2, string? path3, string expected)
    {
        var vsb = new ValueStringBuilder();
        try
        {
            _pgFileSystem.JoinPath(path1.AsSpan(), path2.AsSpan(), path3.AsSpan(), ref vsb);
            var result = vsb.ToString();
            Assert.Equal(expected, result);
#if Windows
            Assert.Equal(result, _fileSystem.Path.Join(path1, path2, path3));
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
