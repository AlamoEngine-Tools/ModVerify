using System;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlSByteParser : PetroglyphNumberParser<sbyte>
{
    public static readonly PetroglyphXmlSByteParser Instance = new();

    private protected override sbyte DefaultValue => 0;

    internal override int EngineDataTypeId => 0x3;

#if !NET7_0_OR_GREATER
    protected override sbyte MaxValue => sbyte.MaxValue;

    protected override sbyte MinValue => sbyte.MinValue;
#endif

    private PetroglyphXmlSByteParser()
    {
    }

    protected internal override sbyte ParseCore(ReadOnlySpan<char> trimmedValue, XElement element)
    {
        var intValue = PetroglyphXmlIntegerParser.Instance.ParseCore(trimmedValue, element);

        var asSByte = (sbyte)intValue;
        if (intValue != asSByte)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"Expected a byte value (0 - 255) but got value '{intValue}'.",
            });
        }

        return asSByte;
    }
}