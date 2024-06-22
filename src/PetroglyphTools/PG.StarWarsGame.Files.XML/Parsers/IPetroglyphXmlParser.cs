using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlParser
{
    public object Parse(XElement element);
}

public interface IPetroglyphXmlParser<T> : IPetroglyphXmlParser
{
    public new T Parse(XElement element);
}