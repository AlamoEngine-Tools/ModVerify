using System.IO;
using PG.Commons.Collections;
using PG.Commons.Hashing;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlFileContainerParser<T> : IPetroglyphXmlParser where T : notnull 
{
    void ParseFile(Stream xmlStream, IValueListDictionary<Crc32, T> parsedEntries);
}