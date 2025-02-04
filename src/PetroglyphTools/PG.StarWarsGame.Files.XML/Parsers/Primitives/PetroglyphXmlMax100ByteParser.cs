using Microsoft.Extensions.Logging;
using System;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Utilities;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlMax100ByteParser : PetroglyphXmlPrimitiveElementParser<byte>
{
    internal PetroglyphXmlMax100ByteParser(IServiceProvider serviceProvider, IPrimitiveXmlParserErrorListener listener) : base(serviceProvider, listener)
    {
    }

    public override byte Parse(XElement element)
    {
        var intValue = PrimitiveParserProvider.IntParser.Parse(element);

        if (intValue > 100)
            intValue = 100;

        var asByte = (byte)intValue;
        if (intValue != asByte)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"Expected a byte value (0 - 255) but got value '{intValue}'."));
        }

        // Add additional check, cause the PG implementation is broken, but we need to stay "bug-compatible".
        if (asByte > 100)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"Expected a byte value (0 - 100) but got value '{asByte}'."));
        }

        return asByte;
    }

    public byte ParseWithRange(XElement element, byte minValue, byte maxValue)
    {
        if (maxValue > 100) 
            Logger?.LogWarning("Upper bound for clamp range is above 100 for a parser that is meant to be capped to value 100.");

        var value = Parse(element);
        
        var clamped = PGMath.Clamp(value, minValue, maxValue);
        if (value != clamped)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"Expected byte between {minValue} and {maxValue} but got value '{value}'."));
        }
        return clamped;
    }

    protected override void OnParseError(XmlParseErrorEventArgs e)
    {
        Logger?.LogWarning(e.Message);
        base.OnParseError(e);
    }
}