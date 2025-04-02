using System;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;

namespace PG.StarWarsGame.Engine.IO;

internal sealed class GameRepositoryFactory(IServiceProvider serviceProvider) :  IGameRepositoryFactory
{
    public GameRepository Create(GameEngineType engineType, GameLocations gameLocations, GameEngineErrorReporterWrapper errorReporter)
    {
        if (engineType == GameEngineType.Eaw)
            throw new NotImplementedException("Empire at War is currently not supported.");
        return new FocGameRepository(gameLocations, errorReporter, serviceProvider);
    }
}