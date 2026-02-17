using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlUnsignedIntegerParser : PetroglyphPrimitiveXmlParser<uint>
{
    public static readonly PetroglyphXmlUnsignedIntegerParser Instance = new();

    private protected override uint DefaultValue => 0;

    private PetroglyphXmlUnsignedIntegerParser()
    {
    }

    protected internal override uint ParseCore(ReadOnlySpan<char> trimmedValue, XElement element)
    {
        var intValue = PetroglyphXmlIntegerParser.Instance.ParseCore(trimmedValue, element);

        var asUint = (uint)intValue;
        if (intValue != asUint)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"Expected unsigned integer but got '{intValue}'.",
            });
        }

        return asUint;
    }
}