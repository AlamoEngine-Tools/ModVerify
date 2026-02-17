using System.Xml.Linq;
using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlNamedElementParser<T> : IPetroglyphXmlParserInfo where T : notnull
{
    T Parse(XElement element, IReadOnlyFrugalValueListDictionary<Crc32, T> parsedEntries, out Crc32 nameCrc);
}