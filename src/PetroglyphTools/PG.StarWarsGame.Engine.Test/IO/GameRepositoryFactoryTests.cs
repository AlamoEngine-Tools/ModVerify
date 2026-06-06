using System;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Testing;
using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

/// <summary>
/// Tests <see cref="GameRepositoryFactory"/> engine dispatch in isolation. The factory is engine-agnostic
/// infrastructure, so it is a standalone test class rather than an engine-bound repository test.
/// </summary>
public class GameRepositoryFactoryTests : EngineTestBase
{
    private GameRepository Create(GameEngineType engine, VirtualGameRepo repo)
    {
        var factory = new GameRepositoryFactory(ServiceProvider);
        return factory.Create(engine, repo.GameLocations, new GameEngineErrorReporterWrapper(null));
    }

    [Fact]
    public void Create_Foc_ReturnsFocGameRepositoryWithMatchingEngineType()
    {
        using var repo = new VirtualGameRepoBuilder(ServiceProvider).Build();

        var gameRepo = Create(GameEngineType.Foc, repo);

        Assert.IsType<FocGameRepository>(gameRepo);
        Assert.Equal(GameEngineType.Foc, gameRepo.EngineType);
    }

    [Fact]
    public void Create_Eaw_ThrowsNotImplemented()
    {
        using var repo = new VirtualGameRepoBuilder(ServiceProvider).Build();

        Assert.Throws<NotImplementedException>(() => Create(GameEngineType.Eaw, repo));
    }
}
