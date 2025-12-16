namespace ModVerify.CliApp.Test.TargetSelectors;

public class AutomaticSelectorTest : CommonTestBase
{
    //private readonly AutomaticSelector _selector;
    //private readonly IRegistry _registry = new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike);

    //public AutomaticSelectorTest()
    //{
    //    _selector = new AutomaticSelector(ServiceProvider);
    //}

    //protected override void SetupServices(ServiceCollection serviceCollection)
    //{
    //    base.SetupServices(serviceCollection);
    //    serviceCollection.AddSingleton(_registry);
    //}

    //[Fact]
    //public void Test_Select_GameNotInstalled()
    //{
    //    var settings = new VerificationTargetSettings
    //    {
    //        TargetPath = "/test",
    //    };

    //    Assert.Throws<GameNotFoundException>(() => _selector.SelectTarget(settings));
    //}

    //[Theory]
    //[MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    //public void Test_Select_Game(GameIdentity identity)
    //{
    //    var (eaw, foc) = InstallGames(identity.Platform);
    //    var game = identity.Type switch
    //    {
    //        GameType.Eaw => eaw,
    //        GameType.Foc => foc,
    //        _ => throw new ArgumentOutOfRangeException()
    //    };

    //    var settings = new VerificationTargetSettings
    //    {
    //        TargetPath = game.Directory.FullName,
    //    };

    //    var result = _selector.SelectTarget(settings);
    //    Assert.Equal(identity.Type, result.Engine.FromEngineType());
    //    Assert.Equal(game, result.Target);
    //    Assert.Equal(game.Directory.FullName, result.Locations.GamePath);
    //}

    //private (IGame eaw, IGame foc) InstallGames(GamePlatform platform)
    //{
    //    var eaw = FileSystem.InstallGame(new GameIdentity(GameType.Eaw, platform), ServiceProvider);
    //    var foc = FileSystem.InstallGame(new GameIdentity(GameType.Eaw, platform), ServiceProvider);



    //    if (platform == GamePlatform.SteamGold)
    //    {
    //        InstallSteam(eaw, foc);
    //    }

    //    return (eaw, foc);
    //}

    //private void InstallSteam(IGame eaw, IGame foc)
    //{
    //    var registry = ServiceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();
    //    FileSystem.InstallSteam(registry);

    //    // Register Game to Steam
    //    var lib = FileSystem.InstallDefaultLibrary(ServiceProvider);
    //    lib.InstallGame(32470, "Star Wars Empire at War", [32472], SteamAppState.StateFullyInstalled);
    //}
}