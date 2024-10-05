using System;
using System.Numerics;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlVector2FParser : PetroglyphXmlPrimitiveElementParser<Vector2>
{
    internal PetroglyphXmlVector2FParser(IServiceProvider serviceProvider, IPrimitiveXmlParserErrorListener listener) : base(serviceProvider, listener)
    {
    }

    public override Vector2 Parse(XElement element)
    {
        var listOfValues = PrimitiveParserProvider.LooseStringListParser.Parse(element);

        if (listOfValues.Count == 0)
            return default;

        var floatParser = PrimitiveParserProvider.FloatParser;

        if (listOfValues.Count == 1)
        {
            var value = floatParser.Parse(listOfValues[0], element);
            return new Vector2(value, 0.0f);
        }

        var value1 = floatParser.Parse(listOfValues[0], element);
        var value2 = floatParser.Parse(listOfValues[1], element);
        
        return new Vector2(value1, value2);
    }
}