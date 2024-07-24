using System;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlByteParser : PetroglyphXmlPrimitiveElementParser<byte>
{
    internal PetroglyphXmlByteParser(IServiceProvider serviceProvider, IPrimitiveXmlParserErrorListener listener) : base(serviceProvider, listener)
    {
    }

    public override byte Parse(XElement element)
    {
        var intValue = PrimitiveParserProvider.IntParser.Parse(element);

        var asByte = (byte)intValue;
        if (intValue != asByte)
        {
             var location = XmlLocationInfo.FromElement(element);
             OnParseError(new XmlParseErrorEventArgs(location.XmlFile, element, XmlParseErrorKind.InvalidValue,
                 $"Expected a byte value (0 - 255) but got value '{intValue}' at {location}"));
        }

        return asByte;
    }

    protected override void OnParseError(XmlParseErrorEventArgs e)
    {
        Logger?.LogWarning(e.Message);
        base.OnParseError(e);
    }
}