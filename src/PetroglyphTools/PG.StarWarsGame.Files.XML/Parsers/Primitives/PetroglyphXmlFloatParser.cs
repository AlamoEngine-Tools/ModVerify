using System;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlFloatParser : PetroglyphXmlPrimitiveElementParser<float>
{
    internal PetroglyphXmlFloatParser(IServiceProvider serviceProvider, IPrimitiveXmlParserErrorListener listener) : base(serviceProvider, listener)
    {
    }

    public override float Parse(XElement element)
    {
        // The engine always loads FP numbers a long double and then converts that result to float
        if (!double.TryParse(element.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
        {
            var location = XmlLocationInfo.FromElement(element);
            OnParseError(new XmlParseErrorEventArgs(location.XmlFile, element, XmlParseErrorKind.MalformedValue,
                $"Expected double but got value '{element.Value}' at {location}"));
            return 0.0f;
        }
        return (float)doubleValue;
    }

    protected override void OnParseError(XmlParseErrorEventArgs e)
    {
        Logger?.LogWarning(e.Message);
        base.OnParseError(e);
    }
}