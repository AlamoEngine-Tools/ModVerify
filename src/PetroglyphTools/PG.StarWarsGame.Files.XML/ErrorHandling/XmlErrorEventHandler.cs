using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public delegate void XmlErrorEventHandler(IPetroglyphXmlParser parser, XmlParseErrorEventArgs error);