using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.Repositories;

namespace PG.StarWarsGame.Engine.Database;

internal class GameDatabaseService(IServiceProvider serviceProvider) : IGameDatabaseService
{
    public Task<IGameDatabase> InitializeGameAsync(
        GameEngineType targetEngineType, 
        GameLocations locations,
        IDatabaseErrorListener? errorListener = null,
        CancellationToken cancellationToken = default)
    {
        var repoFactory = serviceProvider.GetRequiredService<IGameRepositoryFactory>();

        using var errorListenerWrapper = new DatabaseErrorListenerWrapper(errorListener, serviceProvider);
        var repository = repoFactory.Create(targetEngineType, locations, errorListenerWrapper);

        var gameInitializer = new GameInitializer(repository, serviceProvider);
        return gameInitializer.InitializeAsync(errorListenerWrapper, cancellationToken);
    }
}