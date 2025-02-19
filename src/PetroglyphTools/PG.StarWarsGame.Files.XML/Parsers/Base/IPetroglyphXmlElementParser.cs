using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlElementParser<T> : IPetroglyphXmlParser where T : notnull
{
    T Parse(XElement element);
}