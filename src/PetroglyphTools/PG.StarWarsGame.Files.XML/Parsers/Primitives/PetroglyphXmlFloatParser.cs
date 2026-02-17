using System;
using System.Globalization;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlFloatParser : PetroglyphPrimitiveXmlParser<float>
{
    public static readonly PetroglyphXmlFloatParser Instance = new();

    private protected override float DefaultValue => 0.0f;

    private PetroglyphXmlFloatParser()
    {
    }

    public float ParseAtLeast(XElement element, float minValue)
    {
        var value = Parse(element);
        var corrected = Math.Max(value, minValue);
        if (corrected != value)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"Expected float to be at least {minValue} but got value '{value}'.",
            });
        }

        return corrected;
    }

    protected internal override float ParseCore(ReadOnlySpan<char> trimmedValue, XElement element)
    {
        // The engine always loads FP numbers a long double and then converts that result to float
        if (!double.TryParse(trimmedValue
#if NETSTANDARD2_0
                    .ToString()
#endif
                , NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.MalformedValue,
                Message = $"Expected double but got value '{trimmedValue.ToString()}'.",
            });
            return 0.0f;
        }

        return (float)doubleValue;
    }
}