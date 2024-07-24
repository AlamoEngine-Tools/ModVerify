using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlParser
{
    object Parse(XElement element);
}

public interface IPetroglyphXmlParser<T> : IPetroglyphXmlParser
{
    new T Parse(XElement element);
}