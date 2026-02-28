using System;
using System.Threading;
using System.Threading.Tasks;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;

namespace PG.StarWarsGame.Engine.GameConstants;

internal class GameConstants(
    GameEngineType engineType,
    GameRepository repository, 
    GameEngineErrorReporterWrapper errorReporter, 
    IServiceProvider serviceProvider)
    : GameManagerBase(engineType, repository, errorReporter, serviceProvider), IGameConstants
{
    protected override Task InitializeCoreAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}