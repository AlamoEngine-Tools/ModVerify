using System.IO;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlFileParser<T> where T : notnull
{
    T? ParseFile(Stream xmlStream);
}