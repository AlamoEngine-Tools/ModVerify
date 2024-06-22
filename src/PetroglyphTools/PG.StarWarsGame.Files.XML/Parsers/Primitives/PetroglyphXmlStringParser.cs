using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlStringParser : PetroglyphXmlPrimitiveParser<string>
{
    public static readonly PetroglyphXmlStringParser Instance = new();

    private PetroglyphXmlStringParser()
    {
    }

    public override string Parse(XElement element)
    {
        return element.Value.Trim();
    }
}