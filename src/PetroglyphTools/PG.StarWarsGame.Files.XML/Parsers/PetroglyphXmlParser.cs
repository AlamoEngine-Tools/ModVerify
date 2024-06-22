using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlParser<T> : IPetroglyphXmlParser<T>
{
    public abstract T Parse(XElement element);

    object IPetroglyphXmlParser.Parse(XElement element)
    {
        return Parse(element);
    }
}