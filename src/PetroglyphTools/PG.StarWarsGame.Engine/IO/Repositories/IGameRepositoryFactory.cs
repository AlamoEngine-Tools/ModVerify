using PG.StarWarsGame.Engine.ErrorReporting;

namespace PG.StarWarsGame.Engine.IO.Repositories;

internal interface IGameRepositoryFactory
{
    GameRepository Create(GameEngineType engineType, GameLocations gameLocations, GameErrorReporterWrapper errorReporter);
}