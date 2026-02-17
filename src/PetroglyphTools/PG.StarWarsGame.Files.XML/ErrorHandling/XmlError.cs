using System;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public sealed class XmlError(
    IPetroglyphXmlParserInfo parser,
    XElement? element = null,
    XmlLocationInfo? locationInfo = null)
{
    public XmlLocationInfo FileLocation { get; } = locationInfo ?? (element is not null ? XmlLocationInfo.FromElement(element) : default);

    public IPetroglyphXmlParserInfo Parser { get; } = parser ?? throw new ArgumentNullException(nameof(parser));

    public XElement? Element { get; } = element;

    public required XmlParseErrorKind ErrorKind { get; init; }

    public required string Message { get; init; }
}