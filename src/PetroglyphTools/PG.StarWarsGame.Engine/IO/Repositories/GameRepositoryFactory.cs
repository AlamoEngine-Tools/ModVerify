using System;
using PG.StarWarsGame.Engine.Database.ErrorReporting;

namespace PG.StarWarsGame.Engine.IO.Repositories;

internal sealed class GameRepositoryFactory(IServiceProvider serviceProvider) :  IGameRepositoryFactory
{
    public GameRepository Create(GameEngineType engineType, GameLocations gameLocations, DatabaseErrorReporterWrapper errorReporter)
    {
        if (engineType == GameEngineType.Eaw)
            throw new NotImplementedException("Empire at War is currently not supported.");
        return new FocGameRepository(gameLocations, errorReporter, serviceProvider);
    }
}