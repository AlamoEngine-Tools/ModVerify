using AET.ModVerify.App;
using AET.ModVerify.App.Settings;
using AET.ModVerify.App.Settings.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;
using Testably.Abstractions;
using Xunit;

namespace ModVerify.CliApp.Test;

public class SettingsBuilderTest
{
    private readonly SettingsBuilder _builder;

    public SettingsBuilderTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IFileSystem>(new RealFileSystem());
        var provider = services.BuildServiceProvider();
        _builder = new SettingsBuilder(provider);
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
