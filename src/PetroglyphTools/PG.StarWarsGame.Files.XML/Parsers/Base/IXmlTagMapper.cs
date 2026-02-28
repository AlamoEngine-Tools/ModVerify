using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.Data;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IXmlTagMapper<T> where T : XmlObject
{
    bool TryParseEntry(XElement element, T target, bool replace);
}