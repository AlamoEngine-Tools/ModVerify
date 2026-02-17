using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlStringParser : PetroglyphPrimitiveXmlParser<string>
{
    public static readonly PetroglyphXmlStringParser Instance = new();

    private PetroglyphXmlStringParser()
    {
    }

    private protected override string DefaultValue => string.Empty;

    protected internal override string ParseCore(ReadOnlySpan<char> trimmedValue, XElement element)
    {
        return trimmedValue.ToString();
    }
}