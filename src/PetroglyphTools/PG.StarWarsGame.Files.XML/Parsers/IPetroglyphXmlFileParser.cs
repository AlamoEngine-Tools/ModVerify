using System.IO;
using PG.Commons.Hashing;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlFileParser : IPetroglyphXmlElementParser
{
    public object? ParseFile(Stream stream);
}

public interface IPetroglyphXmlFileParser<T> : IPetroglyphXmlElementParser<T>, IPetroglyphXmlFileParser
{
    public new T ParseFile(Stream stream);

    public void ParseFile(Stream stream, IValueListDictionary<Crc32, T> parsedEntries);
}