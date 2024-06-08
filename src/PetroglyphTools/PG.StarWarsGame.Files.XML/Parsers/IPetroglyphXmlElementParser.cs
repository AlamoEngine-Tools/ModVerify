using System.Xml.Linq;
using PG.Commons.Hashing;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlElementParser
{
    public object Parse(XElement element);
}

public interface IPetroglyphXmlElementParser<T> : IPetroglyphXmlElementParser
{
    public new T Parse(XElement element);

    public T Parse(XElement element, IReadOnlyValueListDictionary<Crc32, T> parsedElements, out Crc32 nameCrc);
}