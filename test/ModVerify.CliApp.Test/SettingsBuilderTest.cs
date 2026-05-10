using AET.ModVerify.App;
using AET.ModVerify.App.Settings;
using AET.ModVerify.App.Settings.CommandLine;
using System.IO.Abstractions;
using Testably.Abstractions.Testing;
using Xunit;
using AnakinRaW.CommonUtilities.Testing;

namespace ModVerify.CliApp.Test;

public class SettingsBuilderTest : TestBaseWithFileSystem
{
    private readonly SettingsBuilder _builder;

    public SettingsBuilderTest()
    {
        _builder = new SettingsBuilder(ServiceProvider);
    }

    protected override IFileSystem CreateFileSystem()
    {
        return new MockFileSystem();
    }

    [Theory]
    [InlineData("path1", "path2")]
    public void BuildSettings_Paths_SplitsCorrectly(string p1, string p2)
    {
        var separator = FileSystem.Path.PathSeparator;
        var paths = $"{p1}{separator}{p2}";
        var expected = new[] { FileSystem.Path.GetFullPath(p1), FileSystem.Path.GetFullPath(p2) };

        var options = new VerifyVerbOption
        {
            ModPaths = paths,
            AdditionalFallbackPath = paths,
            TargetPath = "myPath"
        };

        var settings = _builder.BuildSettings(options);
        
        Assert.Equal(expected, settings.VerificationTargetSettings.ModPaths);
        Assert.Equal(expected, settings.VerificationTargetSettings.AdditionalFallbackPaths);
    }

    [Fact]
    public void BuildSettings_FallbackGamePath_RequiresGamePath()
    {
        var gamePath = "game";
        var fallbackPath = "fallback";

        var options = new VerifyVerbOption
        {
            GamePath = gamePath,
            FallbackGamePath = fallbackPath,
            TargetPath = "myPath"
        };

        var settings = _builder.BuildSettings(options);
        Assert.Equal(FileSystem.Path.GetFullPath(fallbackPath), settings.VerificationTargetSettings.FallbackGamePath);

        var optionsNoGame = new VerifyVerbOption
        {
            FallbackGamePath = fallbackPath,
            TargetPath = "myPath"
        };

        var settingsNoGame = _builder.BuildSettings(optionsNoGame);
        Assert.Null(settingsNoGame.VerificationTargetSettings.FallbackGamePath);
    }

    [Fact]
    public void BuildSettings_UseDefaultBaseline_And_Baseline_Throws()
    {
        var options = new VerifyVerbOption
        {
            UseDefaultBaseline = true,
            Baseline = "myBaseline.json",
            TargetPath = "myPath",
        };

        Assert.Throws<AppArgumentException>(() => _builder.BuildSettings(options));
    }

    [Fact]
    public void BuildSettings_UseDefaultBaseline_And_SearchBaseline_Throws()
    {
        var options = new VerifyVerbOption
        {
            UseDefaultBaseline = true,
            SearchBaselineLocally = true,
            TargetPath = "myPath",
        };

        Assert.Throws<AppArgumentException>(() => _builder.BuildSettings(options));
    }

    [Fact]
    public void BuildSettings_UseDefaultBaseline_Alone_DoesNotThrow()
    {
        var options = new VerifyVerbOption
        {
            UseDefaultBaseline = true,
            TargetPath = "myPath",
        };

        var settings = _builder.BuildSettings(options);
        Assert.NotNull(settings);
    }
}
