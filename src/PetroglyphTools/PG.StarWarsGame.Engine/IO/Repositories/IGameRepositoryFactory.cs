using PG.StarWarsGame.Engine.Database.ErrorReporting;

namespace PG.StarWarsGame.Engine.IO.Repositories;

internal interface IGameRepositoryFactory
{
    GameRepository Create(GameEngineType engineType, GameLocations gameLocations, DatabaseErrorReporterWrapper errorReporter);
}