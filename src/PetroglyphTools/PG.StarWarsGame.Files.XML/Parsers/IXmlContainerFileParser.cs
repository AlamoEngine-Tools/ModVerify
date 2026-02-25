using System.IO;
using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML.Data;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IXmlContainerFileParser<T> : IPetroglyphXmlParserInfo where T : NamedXmlObject
{
    NamedXmlObjectParser<T> ElementParser { get; }

    void ParseFile(Stream xmlStream, IFrugalValueListDictionary<Crc32, T> parsedEntries);
}