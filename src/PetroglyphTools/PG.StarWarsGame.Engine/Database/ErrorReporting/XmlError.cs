using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Database.ErrorReporting;

public sealed class XmlError
{
    public string File { get; init; }

    public string Parser { get; init; }

    public XElement? Element { get; init; }

    public XmlParseErrorKind ErrorKind { get; init; }

    public string Message { get; init; }
}