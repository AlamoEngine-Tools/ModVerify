using System;
using System.Collections.Generic;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlLooseStringListParser : PetroglyphPrimitiveXmlParser<IList<string>>
{
    // These are the characters the engine uses as a generic list separator
    private static readonly char[] Separators =
    [
        ' ',
        ',',
        '\t',
        '\n',
        '\r'
    ];

    public static readonly PetroglyphXmlLooseStringListParser Instance = new();

    private protected override IList<string> DefaultValue => [];

    private PetroglyphXmlLooseStringListParser()
    {
    }

    protected internal override IList<string> ParseCore(string trimmedValue, XElement element)
    {
        if (trimmedValue.Length > 0x2000)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.TooLongData,
                $"Input value is too long '{trimmedValue.Length}' at {XmlLocationInfo.FromElement(element)}"));
            return DefaultValue;
        }

        var entries = trimmedValue.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        return entries;
    }
}