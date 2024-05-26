using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public interface IPetroglyphXmlElementParser
{
    public object? Parse(XElement element);
}

public interface IPetroglyphXmlElementParser<out T> : IPetroglyphXmlElementParser
{
    public new T Parse(XElement element);
}