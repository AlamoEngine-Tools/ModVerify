using System;
using System.Globalization;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlFloatParser : PetroglyphPrimitiveXmlParser<float>
{
    public static readonly PetroglyphXmlFloatParser Instance = new();

    private PetroglyphXmlFloatParser()
    {
    }

    public float Parse(string value, XElement element)
    {
        // The engine always loads FP numbers a long double and then converts that result to float
        if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.MalformedValue,
                $"Expected double but got value '{value}'."));
            return 0.0f;
        }

        return (float)doubleValue;
    }

    public override float Parse(XElement element)
    { 
        return Parse(element.Value, element);
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
}