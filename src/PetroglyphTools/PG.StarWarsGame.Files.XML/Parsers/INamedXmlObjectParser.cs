using System.Xml.Linq;
using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML.Data;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface INamedXmlObjectParser<T> : IPetroglyphXmlParserInfo where T : NamedXmlObject
{
    T Parse(XElement element, IReadOnlyFrugalValueListDictionary<Crc32, T> parsedEntries, out Crc32 nameCrc);
}