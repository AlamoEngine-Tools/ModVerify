using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.Repositories;

namespace PG.StarWarsGame.Engine.Pipeline;

internal class GameDatabaseService(IServiceProvider serviceProvider) : IGameDatabaseService
{
    public async Task<IGameDatabase> CreateDatabaseAsync(GameEngineType targetEngineType, GameLocations locations, CancellationToken cancellationToken = default)
    {
        var repoFactory = serviceProvider.GetRequiredService<IGameRepositoryFactory>();
        var repository = repoFactory.Create(targetEngineType, locations);

        var pipeline = new GameDatabaseCreationPipeline(repository, serviceProvider);
        await pipeline.RunAsync(cancellationToken);
        return pipeline.GameDatabase;
    }
}