using PG.StarWarsGame.Engine.Database.ErrorReporting;

namespace PG.StarWarsGame.Engine.Repositories;

internal interface IGameRepositoryFactory
{
    GameRepository Create(GameEngineType engineType, GameLocations gameLocations, DatabaseErrorListenerWrapper errorListener);
}