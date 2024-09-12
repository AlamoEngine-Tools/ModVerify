using System.Xml.Linq;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Database.ErrorReporting;

public sealed class XmlError
{
    public required XmlLocationInfo FileLocation { get; init; }

    public required string Parser { get; init; }

    public XElement? Element { get; init; }

    public required XmlParseErrorKind ErrorKind { get; init; }

    public required string Message { get; init; }
}