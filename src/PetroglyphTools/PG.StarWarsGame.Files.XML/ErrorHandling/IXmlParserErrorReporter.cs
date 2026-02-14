using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public interface IXmlParserErrorReporter
{
    void Report(IPetroglyphXmlParserInfo parser, XmlParseErrorEventArgs error);
}