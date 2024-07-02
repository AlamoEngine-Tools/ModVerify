using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlBooleanParser : PetroglyphXmlPrimitiveElementParser<bool>
{
    internal PetroglyphXmlBooleanParser(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override bool Parse(XElement element)
    {
        var valueSpan = element.Value.AsSpan();
        var trimmed = valueSpan.Trim();

        if (trimmed.Length == 0)
            return false;

        // Yes! The engine only checks if the values is exact 1 or starts with Tt or Yy
        // At least it's efficient, I guess...
        if (trimmed.Length == 1 && trimmed[0] == '1')
            return true;

        if (trimmed[0] is 'y' or 'Y' or 't' or 'T')
            return true;

        return false;
    }
}