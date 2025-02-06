using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Utilities;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlIntegerParser : PetroglyphPrimitiveXmlParser<int>
{
    public static readonly PetroglyphXmlIntegerParser Instance = new();

    private PetroglyphXmlIntegerParser()
    {
    }

    public override int Parse(XElement element)
    {
        // The engines uses the C++ function std::atoi which is a little more loose.
        // For example the value '123d' get parsed to 123,
        // whereas in C# int.TryParse returns (false, 0)

        if (!int.TryParse(element.Value, out var i))
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.MalformedValue,
                $"Expected integer but got '{element.Value}'."));
            return 0;
        }

        return i;
    }

    public int ParseWithRange(XElement element, int minValue, int maxValue)
    {
        var value = Parse(element);
        var clamped =  PGMath.Clamp(value, minValue, maxValue);
        if (value != clamped)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"Expected integer between {minValue} and {maxValue} but got value '{value}'."));
        }
        return clamped;
    }
}