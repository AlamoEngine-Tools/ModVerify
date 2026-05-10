using System;
using AnakinRaW.CommonUtilities.Testing.Attributes;
using PG.StarWarsGame.Engine.Utilities;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.Utilities;

public class LowLevelPathTests
{
    [Theory]
    [InlineData("C:/Games/EAW/Data/foo.xml",     "C:/Games/EAW",     12)]
    [InlineData("/home/user/eaw/Data/foo.xml",   "/home/user/eaw",   14)]
    [InlineData("C:/Games/EAW/Data/foo.xml",     "C:/Games/EAW/",    13)]
    [InlineData("C:/Games/EAW/Data",             "C:/Games/EAWX",     9)]
    [InlineData("C:/Games",                      "C:/Games/EAW",      8)]
    [InlineData("C:/Games/EAW",                  "C:/Games/EAW",     12)]
    [InlineData("D:/Games/EAW",                  "C:/Games/EAW",      0)]
    [InlineData("",                              "C:/Games/EAW",      0)]
    [InlineData("C:/foo",                        "",                  0)]
    public void GetCommonDirectoryPrefixLength_Cases(string path, string directory, int expected)
    {
        Assert.Equal(expected, LowLevelPath.GetCommonDirectoryPrefixLength(path.AsSpan(), directory.AsSpan()));
    }
    
    [PlatformSpecificTheory(TestPlatformIdentifier.Windows)]
    [InlineData("C:/Games/EAW/Data/foo.xml",     "C:\\Games\\EAW",   12)]
    [InlineData("C:\\Games\\EAW\\Data\\foo.xml", "C:\\Games\\EAW",   12)]
    public void GetCommonDirectoryPrefixLength_BackslashSeparator_Windows(string path, string directory, int expected)
    {
        Assert.Equal(expected, LowLevelPath.GetCommonDirectoryPrefixLength(path.AsSpan(), directory.AsSpan()));
    }

    [PlatformSpecificTheory(TestPlatformIdentifier.Windows)]
    [InlineData("c:/games/eaw/Data/foo.xml",     "C:\\Games\\EAW",   12)]
    [InlineData("C:/GAMES/EAW/Data/foo.xml",     "C:/games/eaw",     12)]
    public void GetCommonDirectoryPrefixLength_CaseInsensitive_Windows(string path, string directory, int expected)
    {
        Assert.Equal(expected, LowLevelPath.GetCommonDirectoryPrefixLength(path.AsSpan(), directory.AsSpan()));
    }
    
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void IsHostFileSystemCaseSensitive_Windows_IsFalse()
    {
        Assert.False(LowLevelPath.IsHostFileSystemCaseSensitive);
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void IsHostFileSystemCaseSensitive_Linux_IsTrue()
    {
        Assert.True(LowLevelPath.IsHostFileSystemCaseSensitive);
    }

    [Theory]
    // Sibling at root level: path and directory diverge before any separator → no shared prefix.
    [InlineData("foo/bar", "baz/qux", 0)]
    // Trailing separator on path side, directory does not have one — must match the no-trailing form.
    [InlineData("a/b/", "a/b", 3)]
    [InlineData("a/b", "a/b/", 3)]
    // Sibling whose name is a prefix of the other — shared prefix is the parent dir, not the
    // longest character match. "a/b" vs "a/ba" must NOT be reported as 3 chars in common.
    [InlineData("a/b", "a/ba", 2)]
    [InlineData("a/ba", "a/b", 2)]
    [InlineData("a/foo", "a/foobar", 2)]
    [InlineData("a/foobar", "a/foo", 2)]
    public void GetCommonDirectoryPrefixLength_BoundaryCases(string path, string directory, int expected)
    {
        Assert.Equal(expected, LowLevelPath.GetCommonDirectoryPrefixLength(path.AsSpan(), directory.AsSpan()));
    }
}
