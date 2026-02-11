using System.IO;
using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlFileContainerParser<T> : IPetroglyphXmlParser where T : notnull 
{
    void ParseFile(Stream xmlStream, IFrugalValueListDictionary<Crc32, T> parsedEntries);
}