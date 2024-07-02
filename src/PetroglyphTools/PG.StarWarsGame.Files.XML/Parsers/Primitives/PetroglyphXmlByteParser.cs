using System;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlByteParser : PetroglyphXmlPrimitiveElementParser<byte>
{
    internal PetroglyphXmlByteParser(IServiceProvider serviceProvider) : base(serviceProvider)
    {

    }

    public override byte Parse(XElement element)
    {
        var intValue = PrimitiveParserProvider.IntParser.Parse(element);

        var asByte = (byte)intValue;
        if (intValue != asByte)
        {
             var location = XmlLocationInfo.FromElement(element); 
             Logger?.LogWarning($"Expected a byte value (0 - 255) but got value '{intValue}' at {location}");
        }

        return asByte;
    }
}