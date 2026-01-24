using AET.Modinfo.Model;
using AET.Modinfo.Spec.Steam;
using AET.ModVerify.App.GameFinder;
using AET.ModVerify.App.Settings;
using AET.ModVerify.App.TargetSelectors;
using AET.ModVerify.App.Utilities;
using AET.SteamAbstraction.Testing;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;
using System;
using Xunit;

namespace ModVerify.CliApp.Test.TargetSelectors;

public class AutomaticSelectorTest : CommonTestBase
{
    private readonly AutomaticSelector _selector;
    private readonly IRegistry _registry = new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike);

    public AutomaticSelectorTest()
    {
        _selector = new AutomaticSelector(ServiceProvider);
    }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton(_registry);
    }

    [Fact]
    public void Test_SelectTarget_GameNotInstalled()
    {
        var settings = new VerificationTargetSettings
        {
            TargetPath = "/test",
        };
        Assert.Throws<TargetNotFoundException>(() => _selector.SelectTarget(settings));
    }
    
    [Fact]
    public void Test_SelectTarget_WrongSettings()
    {
        var settings = new VerificationTargetSettings
        {
            TargetPath = "/test",
            FallbackGamePath = "does/not/exist",
            ModPaths = ["also/does/not/exist"],
            GamePath = "not/found"
        };
        Assert.Throws<ArgumentException>(() => _selector.SelectTarget(settings));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Test_SelectTarget_FromGamePath(IGameIdentity identity)
    {
        TestSelectTarget(
            identity,
            i => i,
            gi => new VerificationTargetSettings
            {
                TargetPath = gi.PlayableObject.Directory.FullName,
                Engine = null
            });
        TestSelectTarget(
            identity,
            i => i,
            gi => new VerificationTargetSettings
            {
                TargetPath = gi.PlayableObject.Directory.FullName,
                Engine = gi.PlayableObject.Game.Type.ToEngineType()
            });
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Test_SelectTarget_FromGamePath_OppositeEngine_ThrowsGameNotFoundException(IGameIdentity identity)
    {
        TestSelectTarget(identity,
            i => i,
            gi => new VerificationTargetSettings
            {
                TargetPath = gi.PlayableObject.Directory.FullName,
                Engine = gi.PlayableObject.Game.Type.Opposite().ToEngineType(),
            },
            typeof(GameNotFoundException));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Test_SelectTarget_ModInModsDir(IGameIdentity identity)
    {
        TestSelectTarget(
            identity,
            gameInstallation => gameInstallation.InstallMod("MyMod", false),
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = null
            });
        TestSelectTarget(
            identity,
            gameInstallation => gameInstallation.InstallMod("MyMod", false),
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = ti.GameInstallation.Game.Type.ToEngineType()
            });
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Test_SelectTarget_ModInModsDir_WrongGameEngine_Throws(IGameIdentity identity)
    {
        TestSelectTarget(
            identity,
            gameInstallation => gameInstallation.InstallMod("MyMod", false),
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = ti.GameInstallation.Game.Type.Opposite().ToEngineType()
            }, typeof(ArgumentException));
    }


    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Test_SelectTarget_Workshops_UnknownModEngineType(GameType gameType)
    {
        var identity = new GameIdentity(gameType, GamePlatform.SteamGold);
        TestSelectTarget(
            identity,
            gameInstallation => gameInstallation.InstallMod("MyMod", true),
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = ti.GameInstallation.Game.Type.ToEngineType()
            });

        TestSelectTarget(
            identity,
            gameInstallation => gameInstallation.InstallMod("MyMod", true),
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = null // Causes fallback to FoC
            },
            overrideAssertData: new OverrideAssertData { GameType = GameType.Foc });
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Test_SelectTarget_Workshops_UnspecifiedEngineIsCorrectEngineTyp_KnownModEngineType(GameType gameType)
    {
        var identity = new GameIdentity(gameType, GamePlatform.SteamGold);

        var modinfo = new ModinfoData("MyMod")
        {
            SteamData = new SteamData("123456", "123456", SteamWorkshopVisibility.Public, "MyMod",
                [gameType.ToString().ToUpper()])
        };

        TestSelectTarget(
            identity,
            gameInstallation =>
            {
                var modInstallation = gameInstallation.InstallMod(modinfo, true);
                modInstallation.InstallModinfoFile(modinfo);
                return modInstallation;
            },
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = null
            });
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Test_SelectTarget_Workshops_IncompatibleKnownModEngineType_Throws(GameType gameType)
    {
        var identity = new GameIdentity(gameType, GamePlatform.SteamGold);

        var modinfo = new ModinfoData("MyMod")
        {
            SteamData = new SteamData("123456", "123456", SteamWorkshopVisibility.Public, "MyMod",
                [gameType.ToString().ToUpper()])
        };

        TestSelectTarget(
            identity,
            gameInstallation =>
            {
                var modInstallation = gameInstallation.InstallMod(modinfo, true);
                modInstallation.InstallModinfoFile(modinfo);
                return modInstallation;
            },
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = gameType.Opposite().ToEngineType()
            },
            typeof(ArgumentException));
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Test_SelectTarget_Workshops_MultipleKnownModEngineTypes(GameType gameType)
    {
        var identity = new GameIdentity(gameType, GamePlatform.SteamGold);

        var modinfo = new ModinfoData("MyMod")
        {
            SteamData = new SteamData("123456", "123456", SteamWorkshopVisibility.Public, "MyMod",
                [gameType.ToString().ToUpper(), gameType.Opposite().ToString().ToUpper()])
        };

        TestSelectTarget(
            identity,
            gameInstallation =>
            {
                var modInstallation = gameInstallation.InstallMod(modinfo, true);
                modInstallation.InstallModinfoFile(modinfo);
                return modInstallation;
            },
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = gameType.ToEngineType()
            });

        TestSelectTarget(
            identity,
            gameInstallation =>
            {
                var modInstallation = gameInstallation.InstallMod(modinfo, true);
                modInstallation.InstallModinfoFile(modinfo);
                return modInstallation;
            },
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = null  // Causes fallback to FoC
            }, 
            overrideAssertData: new OverrideAssertData{GameType = GameType.Foc});
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Test_SelectTarget_DetachedMod_NoEngineSpecified_Throws(IGameIdentity identity)
    {
        TestSelectTarget(
            identity,
            gameInstallation =>
            {
                var modinfo = new ModinfoData("DetachedMod");
                var modPath = FileSystem.Directory.CreateDirectory("/detachedMod");
                return gameInstallation.InstallMod(modinfo, modPath, false);
            },
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = null // No Engine means we cannot proceed
            },
            expectedExceptionType: typeof(ArgumentException));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Test_SelectTarget_DetachedMod(IGameIdentity identity)
    {
        TestSelectTarget(
            identity,
            gameInstallation =>
            {
                var modinfo = new ModinfoData("DetachedMod");
                var modPath = FileSystem.Directory.CreateDirectory("/detachedMod");
                return gameInstallation.InstallMod(modinfo, modPath, false);
            },
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = identity.Type.ToEngineType()
            });
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Test_SelectTarget_AttachedMod_DoesNotExist(IGameIdentity identity)
    {
        // Currently, only Steam is supported for detached mods.
        TestSelectTarget(
            identity,
            gameInstallation =>
            {
                var modInstallation = gameInstallation.InstallMod("MyMod", GITestUtilities.GetRandomWorkshopFlag(identity));
                modInstallation.Mod.Directory.Delete(true);
                return modInstallation;
            },
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = identity.Type.ToEngineType()
            },
            typeof(TargetNotFoundException));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Test_SelectTarget_DetachedMod_DoesNotExist(IGameIdentity identity)
    {
        TestSelectTarget(
            identity,
            gameInstallation =>
            {
                var modinfo = new ModinfoData("DetachedMod");
                var modPath = FileSystem.DirectoryInfo.New("/detachedMod");
                var modInstallation = gameInstallation.InstallMod(modinfo, modPath, false);
                modPath.Delete(true);
                return modInstallation;
            },
            ti => new VerificationTargetSettings
            {
                TargetPath = ti.PlayableObject.Directory.FullName,
                Engine = identity.Type.ToEngineType()
            },
            typeof(TargetNotFoundException));
    }

    private void TestSelectTarget(
        IGameIdentity identity,
        Func<ITestingGameInstallation, ITestingPhysicalPlayableObjectInstallation> targetFactory,
        Func<ITestingPhysicalPlayableObjectInstallation, VerificationTargetSettings> settingsFactory,
        Type? expectedExceptionType = null,
        OverrideAssertData? overrideAssertData = null)
    {
        var (eaw, foc) = InstallGames(identity.Platform);
        var gameInstallation = identity.Type switch
        {
            GameType.Eaw => eaw,
            GameType.Foc => foc,
            _ => throw new ArgumentOutOfRangeException()
        };

        var targetInstallation = targetFactory(gameInstallation);
        
        var settings = settingsFactory(targetInstallation);

        if (expectedExceptionType is not null)
        {
            Assert.Throws(expectedExceptionType, () => _selector.SelectTarget(settings));
            return;
        }

        var result = _selector.SelectTarget(settings);
        Assert.Equal(overrideAssertData?.GameType ?? identity.Type, result.Engine.FromEngineType());
        Assert.Equal(targetInstallation.PlayableObject.GetType(), result.Target!.GetType());
        Assert.Equal(targetInstallation.PlayableObject.Directory.FullName, result.Locations.TargetPath);
        
        if (result.Engine == GameEngineType.Foc)
            Assert.NotEmpty(result.Locations.FallbackPaths);
        else
            Assert.Empty(result.Locations.FallbackPaths);
    }

    private (ITestingGameInstallation eaw, ITestingGameInstallation foc) InstallGames(GamePlatform platform)
    {
        var eaw = GameInfrastructureTesting.Game(new GameIdentity(GameType.Eaw, platform), ServiceProvider);
        var foc = GameInfrastructureTesting.Game(new GameIdentity(GameType.Foc, platform), ServiceProvider);

        GameInfrastructureTesting.Registry(ServiceProvider).CreateInstalled(eaw.Game);
        GameInfrastructureTesting.Registry(ServiceProvider).CreateInstalled(foc.Game);

        eaw.InstallMod("OtherEawMod");
        foc.InstallMod("OtherFocMod");
        
        if (platform == GamePlatform.SteamGold) 
            InstallSteam();

        return (eaw, foc);
    }

    private void InstallSteam()
    {
        var steam = SteamTesting.Steam(ServiceProvider);
        steam.Install();
        // Register Game to Steam
        var lib = steam.InstallDefaultLibrary();
        lib.InstallGame(32470, "Star Wars Empire at War", [32472]);
    }

    private class OverrideAssertData
    {
        public GameType? GameType { get; init; }
    }
}