using System;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlUnsignedIntegerParser : PetroglyphXmlPrimitiveElementParser<uint>
{
    internal PetroglyphXmlUnsignedIntegerParser(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override uint Parse(XElement element)
    {
        var intValue = PrimitiveParserProvider.IntParser.Parse(element);

        var asUint = (uint)intValue;
        if (intValue != asUint)
        {
            var location = XmlLocationInfo.FromElement(element);
            Logger?.LogWarning($"Expected unsigned integer but got '{intValue}' at {location}");
        }

        return asUint;
    }
}