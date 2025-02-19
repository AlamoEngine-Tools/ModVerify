using System.IO;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlFileParser<T> : IPetroglyphXmlParser where T : notnull
{
    T? ParseFile(Stream xmlStream);
}