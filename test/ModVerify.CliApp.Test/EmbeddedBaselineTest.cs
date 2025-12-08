using AET.ModVerify.App;
using AET.ModVerify.App.Reporting;
using AET.ModVerify.App.Settings;
using AET.ModVerify.Settings;
using AnakinRaW.ApplicationBase.Environment;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine;
using System;
using System.IO.Abstractions;
using ModVerify.CliApp.Test.TestData;
using Testably.Abstractions;

namespace ModVerify.CliApp.Test;

public class BaselineSelectorTest
{
    private static readonly IFileSystem FileSystem = new RealFileSystem();
    private static readonly ModVerifyAppSettings TestSettings = new()
    {
        ReportSettings = new(),
        GameInstallationsSettings = new (),
        VerifyPipelineSettings = new()
        {
            GameVerifySettings = new GameVerifySettings(),
            VerifiersProvider = new NoVerifierProvider()
        }
    };

    private readonly IServiceProvider _serviceProvider;

    public BaselineSelectorTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton(FileSystem);
        sc.AddSingleton<ApplicationEnvironment>(new ModVerifyAppEnvironment(typeof(ModVerifyAppEnvironment).Assembly, FileSystem));
        _serviceProvider = sc.BuildServiceProvider();
    }

    [Theory]
    // [InlineData(GameEngineType.Eaw)] TODO EaW is currently not supported
    [InlineData(GameEngineType.Foc)]
    public void LoadEmbeddedBaseline(GameEngineType engineType)
    {
        // Ensure this operation does not crash, meaning the embedded baseline is at least compatible.
        new BaselineSelector(TestSettings, _serviceProvider).LoadEmbeddedBaseline(engineType);
    }
}