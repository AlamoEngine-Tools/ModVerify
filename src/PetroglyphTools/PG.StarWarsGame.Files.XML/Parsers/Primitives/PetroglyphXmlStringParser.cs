using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlStringParser : PetroglyphXmlPrimitiveElementParser<string>
{
    internal PetroglyphXmlStringParser(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override string Parse(XElement element)
    {
        return element.Value.Trim();
    }
}