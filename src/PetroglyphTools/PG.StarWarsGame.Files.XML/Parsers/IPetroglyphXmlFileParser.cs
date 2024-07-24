using System.IO;
using PG.Commons.Hashing;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlFileParser : IPetroglyphXmlParser
{
    public object? ParseFile(Stream stream);
}

public interface IPetroglyphXmlFileParser<T> : IPetroglyphXmlParser<T>, IPetroglyphXmlFileParser
{
    public new T ParseFile(Stream stream);

    public void ParseFile(Stream stream, IValueListDictionary<Crc32, T> parsedEntries);
}