using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml;

public interface IPetroglyphXmlFileParserFactory
{
    IPetroglyphXmlFileParser<T> CreateFileParser<T>(IXmlParserErrorListener? listener = null);
}