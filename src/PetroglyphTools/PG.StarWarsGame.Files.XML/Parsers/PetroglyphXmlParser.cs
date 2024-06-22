using System.Xml.Linq;
using PG.Commons.Hashing;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlParser<T> : IPetroglyphXmlElementParser<T>
{
    public abstract T Parse(XElement element, IReadOnlyValueListDictionary<Crc32, T> parsedElements, out Crc32 nameCrc);

    public abstract T Parse(XElement element);

    object IPetroglyphXmlElementParser.Parse(XElement element)
    {
        return Parse(element);
    }
}