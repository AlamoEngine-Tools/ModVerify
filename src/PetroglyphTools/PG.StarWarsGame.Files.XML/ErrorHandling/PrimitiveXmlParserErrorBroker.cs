using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Files.XML.ErrorHandling;

internal class PrimitiveXmlParserErrorBroker : IPrimitiveXmlParserErrorListener
{
    public event XmlErrorEventHandler? XmlParseError;

    public void OnXmlParseError(IPetroglyphXmlParser parser, XmlParseErrorEventArgs error)
    {
        XmlParseError?.Invoke(parser, error);
    }
}