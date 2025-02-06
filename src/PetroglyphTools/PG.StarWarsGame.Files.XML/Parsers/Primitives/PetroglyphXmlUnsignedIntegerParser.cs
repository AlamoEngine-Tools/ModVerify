using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlUnsignedIntegerParser : PetroglyphPrimitiveXmlParser<uint>
{
    public static readonly PetroglyphXmlUnsignedIntegerParser Instance = new();

    private PetroglyphXmlUnsignedIntegerParser()
    {
    }

    public override uint Parse(XElement element)
    {
        var intValue = PetroglyphXmlIntegerParser.Instance.Parse(element);

        var asUint = (uint)intValue;
        if (intValue != asUint)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue, 
                $"Expected unsigned integer but got '{intValue}'."));
        }

        return asUint;
    }
}