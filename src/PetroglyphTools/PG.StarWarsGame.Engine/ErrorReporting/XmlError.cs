using System.Xml.Linq;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.ErrorReporting;

public sealed class XmlError
{
    public required XmlLocationInfo FileLocation { get; init; }

    public required IPetroglyphXmlParser Parser { get; init; }

    public XElement? Element { get; init; }

    public required XmlParseErrorKind ErrorKind { get; init; }

    public required string Message { get; init; }
}