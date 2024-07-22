using Microsoft.Extensions.Logging;
using System;
using System.Xml.Linq;

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

            OnParseError(new ParseErrorEventArgs(location.XmlFile, element, XmlParseErrorKind.InvalidValue,
                $"Expected unsigned integer but got '{intValue}' at {location}"));
        }

        return asUint;
    }

    protected override void OnParseError(ParseErrorEventArgs e)
    {
        Logger?.LogWarning(e.Message);
        base.OnParseError(e);
    }
}