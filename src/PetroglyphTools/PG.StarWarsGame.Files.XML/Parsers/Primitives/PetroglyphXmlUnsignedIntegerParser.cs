using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlUnsignedIntegerParser : PetroglyphPrimitiveXmlParser<uint>
{
    public static readonly PetroglyphXmlUnsignedIntegerParser Instance = new();

    private protected override uint DefaultValue => 0;

    private PetroglyphXmlUnsignedIntegerParser()
    {
    }

    protected internal override uint ParseCore(string trimmedValue, XElement element)
    {
        var intValue = PetroglyphXmlIntegerParser.Instance.ParseCore(trimmedValue, element);

        var asUint = (uint)intValue;
        if (intValue != asUint)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"Expected unsigned integer but got '{intValue}'."));
        }

        return asUint;
    }
}