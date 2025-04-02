using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;

namespace PG.StarWarsGame.Engine.IO;

internal interface IGameRepositoryFactory
{
    GameRepository Create(GameEngineType engineType, GameLocations gameLocations, GameEngineErrorReporterWrapper errorReporter);
}