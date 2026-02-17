using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlByteParser : PetroglyphPrimitiveXmlParser<byte>
{
    public static readonly PetroglyphXmlByteParser Instance = new();

    private PetroglyphXmlByteParser()
    {
    }

    private protected override byte DefaultValue => 0;

    protected internal override byte ParseCore(ReadOnlySpan<char> trimmedValue, XElement element)
    {
        var intValue = PetroglyphXmlIntegerParser.Instance.ParseCore(trimmedValue, element);

        var asByte = (byte)intValue;
        if (intValue != asByte)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"Expected a byte value (0 - 255) but got value '{intValue}'.",
            });
        }

        return asByte;
    }
}