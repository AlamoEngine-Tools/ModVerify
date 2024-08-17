using System;
using System.Threading;
using System.Threading.Tasks;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.Repositories;

namespace PG.StarWarsGame.Engine.GameManagers;

internal class GameConstants(GameRepository repository, IServiceProvider serviceProvider)
    : GameManagerBase(repository, serviceProvider), IGameConstants
{
    protected override Task InitializeCoreAsync(DatabaseErrorListenerWrapper errorListener, CancellationToken token)
    {
        return Task.CompletedTask;
    }
}