using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml;

public interface IPetroglyphXmlFileParserFactory
{
    IPetroglyphXmlFileContainerParser<T> CreateFileParser<T>(IXmlParserErrorReporter? errorReporter) where T : notnull;
}