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
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"Expected float to be at least {minValue} but got value '{value}'."));
        }

        return corrected;
    }

    protected internal override float ParseCore(string trimmedValue, XElement element)
    {
        // The engine always loads FP numbers a long double and then converts that result to float
        if (!double.TryParse(trimmedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.MalformedValue,
                $"Expected double but got value '{trimmedValue}'."));
            return 0.0f;
        }

        return (float)doubleValue;
    }
}