using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlByteParser : PetroglyphPrimitiveXmlParser<byte>
{
    public static readonly PetroglyphXmlByteParser Instance = new();

    private PetroglyphXmlByteParser()
    {
    }

    private protected override byte DefaultValue => 0;

    protected internal override byte ParseCore(string trimmedValue, XElement element)
    {
        var intValue = PetroglyphXmlIntegerParser.Instance.ParseCore(trimmedValue, element);

        var asByte = (byte)intValue;
        if (intValue != asByte)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"Expected a byte value (0 - 255) but got value '{intValue}'."));
        }

        return asByte;
    }
}