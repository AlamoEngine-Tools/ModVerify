using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlUnsignedIntegerParser : PetroglyphNumberParser<uint>
{
    public static readonly PetroglyphXmlUnsignedIntegerParser Instance = new();

    private protected override uint DefaultValue => 0;

    internal override int EngineDataTypeId => 0x5;

#if !NET7_0_OR_GREATER
    protected override uint MaxValue => uint.MaxValue;

    protected override uint MinValue => uint.MinValue;
#endif

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