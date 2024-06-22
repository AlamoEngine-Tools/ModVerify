using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml;

public interface IPetroglyphXmlFileParserFactory
{
    IPetroglyphXmlFileParser<T> GetFileParser<T>();
}