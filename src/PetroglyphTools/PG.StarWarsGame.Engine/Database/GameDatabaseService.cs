using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;

namespace PG.StarWarsGame.Engine.Database;

internal class GameDatabaseService(IServiceProvider serviceProvider) : IGameDatabaseService
{
    public Task<IGameDatabase> InitializeGameAsync(
        GameInitializationOptions gameInitializationOptions,
        CancellationToken cancellationToken = default)
    {
        var repoFactory = serviceProvider.GetRequiredService<IGameRepositoryFactory>();

        using var errorListenerWrapper = new GameErrorReporterWrapper(gameInitializationOptions.GameErrorReporter, serviceProvider);
        var repository = repoFactory.Create(gameInitializationOptions.TargetEngineType, gameInitializationOptions.Locations, errorListenerWrapper);

        var gameInitializer = new GameInitializer(repository, gameInitializationOptions.CancelOnError, serviceProvider);
        return gameInitializer.InitializeAsync(errorListenerWrapper, cancellationToken);
    }
}