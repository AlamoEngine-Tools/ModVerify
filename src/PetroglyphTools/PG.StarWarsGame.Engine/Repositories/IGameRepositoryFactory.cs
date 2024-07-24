using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Repositories;

internal interface IGameRepositoryFactory
{
    GameRepository Create(GameEngineType engineType, GameLocations gameLocations, IXmlParserErrorListener? xmlParserErrorListener);
}