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
}
