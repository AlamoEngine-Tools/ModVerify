using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlStringParser(IServiceProvider serviceProvider)
    : PetroglyphXmlElementParser<string>(serviceProvider)
{
    public static readonly PetroglyphXmlStringParser Instance = new();

    private PetroglyphXmlStringParser() : this(null!)
    {
    }

    public override string Parse(XElement element)
    {
        return element.Value.Trim();
    }
}