using System;
using System.Threading;
using System.Threading.Tasks;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;

namespace PG.StarWarsGame.Engine.GameConstants;

internal class GameConstants(GameRepository repository, GameEngineErrorReporterWrapper errorReporter, IServiceProvider serviceProvider)
    : GameManagerBase(repository, errorReporter, serviceProvider), IGameConstants
{
    protected override Task InitializeCoreAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}