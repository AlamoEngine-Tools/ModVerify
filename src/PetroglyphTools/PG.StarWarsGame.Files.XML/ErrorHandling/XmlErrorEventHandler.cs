using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public delegate void XmlErrorEventHandler(IPetroglyphXmlParserInfo parser, XmlParseErrorEventArgs error);