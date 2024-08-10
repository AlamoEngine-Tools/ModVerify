﻿using PG.StarWarsGame.Engine.Database;

namespace PG.StarWarsGame.Engine.Repositories;

internal interface IGameRepositoryFactory
{
    GameRepository Create(GameEngineType engineType, GameLocations gameLocations, DatabaseErrorListenerWrapper errorListener);
}