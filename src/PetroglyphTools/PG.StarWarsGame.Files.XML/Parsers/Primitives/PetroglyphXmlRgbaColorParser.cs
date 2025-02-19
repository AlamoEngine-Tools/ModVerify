using System;
using System.Xml.Linq;
using PG.Commons.Numerics;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlRgbaColorParser : PetroglyphPrimitiveXmlParser<Vector4Int>
{
    public static readonly PetroglyphXmlRgbaColorParser Instance = new();

    private protected override Vector4Int DefaultValue => default;

    private PetroglyphXmlRgbaColorParser()
    {
    }

    protected internal override Vector4Int ParseCore(string trimmedValue, XElement element)
    {
        var values = PetroglyphXmlLooseStringListParser.Instance.ParseCore(trimmedValue, element);

        if (values.Count == 0)
            return DefaultValue;

        Span<int> intValues = stackalloc int[4] { 0, 0, 0, 0 };
        for (var i = 0; i < intValues.Length; i++)
        {
            if (values.Count <= i)
                break;
            intValues[i] = PetroglyphXmlIntegerParser.Instance.ParseCore(values[i], element);
        }

        return new Vector4Int(intValues);
    }
}