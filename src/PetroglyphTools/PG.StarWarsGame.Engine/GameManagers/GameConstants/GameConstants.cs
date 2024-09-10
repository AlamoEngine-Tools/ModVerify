using System;
using System.Threading;
using System.Threading.Tasks;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.Repositories;

namespace PG.StarWarsGame.Engine.GameManagers;

internal class GameConstants(GameRepository repository, DatabaseErrorListenerWrapper errorListener, IServiceProvider serviceProvider)
    : GameManagerBase(repository, errorListener, serviceProvider), IGameConstants
{
    protected override Task InitializeCoreAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}