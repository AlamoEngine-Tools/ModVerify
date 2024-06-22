namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlElementParser : IPetroglyphXmlParser;

public interface IPetroglyphXmlElementParser<T> : IPetroglyphXmlElementParser, IPetroglyphXmlParser<T>;