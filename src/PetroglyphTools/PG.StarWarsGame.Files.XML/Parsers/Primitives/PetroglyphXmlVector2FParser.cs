using System.Numerics;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlVector2FParser : PetroglyphPrimitiveXmlParser<Vector2>
{
    public static readonly PetroglyphXmlVector2FParser Instance = new();

    private static readonly PetroglyphXmlFloatParser FloatParser = PetroglyphXmlFloatParser.Instance;
    private static readonly PetroglyphXmlLooseStringListParser LooseStringListParser = PetroglyphXmlLooseStringListParser.Instance;

    private PetroglyphXmlVector2FParser()
    {
    }

    public override Vector2 Parse(XElement element)
    {
        var listOfValues = LooseStringListParser.Parse(element);

        if (listOfValues.Count == 0)
            return default;

        if (listOfValues.Count == 1)
        {
            var value = FloatParser.Parse(listOfValues[0], element);
            return new Vector2(value, 0.0f);
        }

        var value1 = FloatParser.Parse(listOfValues[0], element);
        var value2 = FloatParser.Parse(listOfValues[1], element);
        
        return new Vector2(value1, value2);
    }
}