using System;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;
using Xunit;

namespace PG.StarWarsGame.Engine.Test.IO;

public abstract class GameRepositoryFactoryTests : EngineRepositoryTestBase
{
    [Fact]
    public void Create_ReportsMatchingEngineType()
    {
        using var repo = CreateBuilder().Build();
        var gameRepo = CreateRepository(repo);

        Assert.Equal(Engine, gameRepo.EngineType);
    }

    [Fact]
    public void Create_EawEngine_ThrowsNotImplemented()
    {
        // Cross-engine: assert the factory rejects EaW today, regardless of the current engine.
        // Once EaW lands this test should be removed (or relocated to the EaW concrete class as
        // a "returns EawGameRepository" assertion).
        using var repo = CreateBuilder().Build();
        var factory = new GameRepositoryFactory(ServiceProvider);

        Assert.Throws<NotImplementedException>(() =>
            factory.Create(GameEngineType.Eaw, repo.GameLocations, new GameEngineErrorReporterWrapper(null)));
    }
}
