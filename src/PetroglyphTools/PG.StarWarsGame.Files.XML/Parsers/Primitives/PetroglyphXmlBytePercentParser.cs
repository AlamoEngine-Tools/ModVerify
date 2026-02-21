using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlBytePercentParser : PetroglyphNumberParser<byte>
{
    public static readonly PetroglyphXmlBytePercentParser Instance = new();

    private protected override byte DefaultValue => 0;

    internal override int EngineDataTypeId => 0x4;

    protected override byte MaxValue => 100;

#if !NET7_0_OR_GREATER
    protected override byte MinValue => byte.MinValue;
#endif

    private PetroglyphXmlBytePercentParser()
    {
    }
    
    protected internal override byte ParseCore(ReadOnlySpan<char> trimmedValue, XElement element)
    {
        var intValue = PetroglyphXmlIntegerParser.Instance.ParseCore(trimmedValue, element);

        if (intValue > MaxValue)
            intValue = MaxValue;

        var asByte = (byte)intValue;
        // Add additional check (> 100), cause the PG implementation is broken, but we need to stay "bug-compatible".
        if (intValue != asByte)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"Expected a byte value (0 - 100) but got value '{asByte}'.",
            });
        }

        return asByte;
    }
}