using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlStringParser : PetroglyphPrimitiveXmlParser<string>
{
    public static readonly PetroglyphXmlStringParser Instance = new();

    private protected override string DefaultValue => string.Empty;

    internal override int EngineDataTypeId => 0x17 & 0x1D & 0x1F;

    private PetroglyphXmlStringParser()
    {
    }

    protected internal override string ParseCore(ReadOnlySpan<char> trimmedValue, XElement element)
    {
        return trimmedValue.ToString();
    }
}