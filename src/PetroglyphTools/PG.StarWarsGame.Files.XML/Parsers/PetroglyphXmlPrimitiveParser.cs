using System;
using System.Xml.Linq;
using PG.Commons.Hashing;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlPrimitiveParser<T> : PetroglyphXmlParser<T>
{
    public sealed override T Parse(XElement element, IReadOnlyValueListDictionary<Crc32, T> parsedElements,
        out Crc32 nameCrc)
    {
        throw new NotSupportedException();
    }
}