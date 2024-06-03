using System;

namespace PG.StarWarsGame.Engine.Repositories;

internal sealed class GameRepositoryFactory(IServiceProvider serviceProvider) :  IGameRepositoryFactory
{
    public GameRepository Create(GameEngineType engineType, GameLocations gameLocations)
    {
        if (engineType == GameEngineType.Eaw)
            throw new NotImplementedException("Empire at War is currently not supported.");
        return new FocGameRepository(gameLocations, serviceProvider);
    }
}