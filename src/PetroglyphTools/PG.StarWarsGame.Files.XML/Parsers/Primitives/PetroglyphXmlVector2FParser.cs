using System.Numerics;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlVector2FParser : PetroglyphPrimitiveXmlParser<Vector2>
{
    public static readonly PetroglyphXmlVector2FParser Instance = new();

    private static readonly PetroglyphXmlFloatParser FloatParser = PetroglyphXmlFloatParser.Instance;

    private static readonly PetroglyphXmlLooseStringListParser LooseStringListParser = PetroglyphXmlLooseStringListParser.Instance;

    private PetroglyphXmlVector2FParser()
    {
    }

    private protected override Vector2 DefaultValue => default;

    protected internal override Vector2 ParseCore(string trimmedValue, XElement element)
    {
        var listOfValues = LooseStringListParser.Parse(element);

        if (listOfValues.Count == 0)
            return default;

        if (listOfValues.Count == 1)
        {
            var value = FloatParser.ParseCore(listOfValues[0], element);
            return new Vector2(value, 0.0f);
        }

        var value1 = FloatParser.ParseCore(listOfValues[0], element);
        var value2 = FloatParser.ParseCore(listOfValues[1], element);

        return new Vector2(value1, value2);
    }
}