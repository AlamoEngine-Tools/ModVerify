using System.IO;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlFileParser : IPetroglyphXmlElementParser
{
    public object? ParseFile(Stream stream);
}

public interface IPetroglyphXmlFileParser<out T> : IPetroglyphXmlElementParser<T>, IPetroglyphXmlFileParser
{
    public new T ParseFile(Stream stream);
}