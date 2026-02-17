using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Utilities;
using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlMax100ByteParser : PetroglyphPrimitiveXmlParser<byte>
{
    public static readonly PetroglyphXmlMax100ByteParser Instance = new();

    private protected override byte DefaultValue => 0;

    private PetroglyphXmlMax100ByteParser()
    {
    }

    protected internal override byte ParseCore(ReadOnlySpan<char> trimmedValue, XElement element)
    {
        var intValue = PetroglyphXmlIntegerParser.Instance.ParseCore(trimmedValue, element);

        if (intValue > 100)
            intValue = 100;

        var asByte = (byte)intValue;
        if (intValue != asByte)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"Expected a byte value (0 - 255) but got value '{intValue}'.",
            });
        }

        // Add additional check, cause the PG implementation is broken, but we need to stay "bug-compatible".
        if (asByte > 100)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"Expected a byte value (0 - 100) but got value '{asByte}'.",
            });
        }

        return asByte;
    }

    public byte ParseWithRange(XElement element, byte minValue, byte maxValue)
    {
        if (maxValue > 100)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"The provided maxValue '{maxValue}' is above 100.",
            });
        }

        // TODO: Do we need to coerce maxValue???

        var value = Parse(element);
        
        var clamped = PGMath.Clamp(value, minValue, maxValue);
        if (value != clamped)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"Expected byte between {minValue} and {maxValue} but got value '{value}'.",
            });
        }
        return clamped;
    }
}