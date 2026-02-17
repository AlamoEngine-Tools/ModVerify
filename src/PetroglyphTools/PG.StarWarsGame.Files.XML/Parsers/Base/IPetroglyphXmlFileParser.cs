using System.IO;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlFileParser<T> : IPetroglyphXmlParserInfo where T : notnull
{
    T ParseFile(Stream xmlStream);
}