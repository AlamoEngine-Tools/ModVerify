using AET.ModVerify.App.GameFinder;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Testing.Extensions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using Xunit;

namespace ModVerify.CliApp.Test.TargetSelectors;

public class GameFinderServiceTest : CommonTestBase
{
    private readonly IRegistry _registry = new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike);
    private readonly GameFinderService _finderService;
    public GameFinderServiceTest()
    {
        _finderService = new GameFinderService(ServiceProvider);
    }
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton(_registry);
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void FindGame_SearchFallbackGame_FocInstalledButEawNot_ThrowsGameNotFoundException(IGameIdentity identity)
    {
        if (identity.Type == GameType.Eaw)
            return;

        var foc = GameInfrastructureTesting.Game(identity, ServiceProvider);
        GameInfrastructureTesting.Registry(ServiceProvider).CreateInstalled(foc.Game);

        Assert.Throws<GameNotFoundException>(() => _finderService.FindGame(foc.Game.Directory.FullName, new GameFinderSettings
        {
            Engine = GameEngineType.Foc,
            SearchFallbackGame = true
        }));

        Assert.Throws<GameNotFoundException>(() => _finderService.FindGame(foc.Game.Directory.FullName, new GameFinderSettings
        {
            Engine = null,
            SearchFallbackGame = true
        }));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void FindGame_EngineEaw_SearchFallbackGameIsIgnored(IGameIdentity identity)
    {
        if (identity.Type == GameType.Foc)
            return;

        var eaw = GameInfrastructureTesting.Game(identity, ServiceProvider);
        GameInfrastructureTesting.Registry(ServiceProvider).CreateInstalled(eaw.Game);

        Assert.DoesNotThrow(() => _finderService.FindGame(eaw.Game.Directory.FullName, new GameFinderSettings
        {
            Engine = GameEngineType.Eaw,
            SearchFallbackGame = true
        }));

        Assert.DoesNotThrow(() => _finderService.FindGame(eaw.Game.Directory.FullName, new GameFinderSettings
        {
            Engine = null,
            SearchFallbackGame = true
        }));
    }
}