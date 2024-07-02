using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlFloatParser : PetroglyphXmlPrimitiveElementParser<float>
{
    internal PetroglyphXmlFloatParser(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override float Parse(XElement element)
    {
        // The engine always loads FP numbers a long double and then converts that result to float
        if (!double.TryParse(element.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
        {
            var location = XmlLocationInfo.FromElement(element);
            Logger?.LogWarning($"Expected double but got value '{element.Value}' at {location}");
            return 0.0f;
        }
        return (float)doubleValue;
    }
}