using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public interface IXmlParserErrorListener
{
    public void OnXmlParseError(IPetroglyphXmlParser parser, XmlParseErrorEventArgs error);
}