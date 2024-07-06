using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlLooseStringListParser : PetroglyphXmlPrimitiveElementParser<IList<string>>
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
    
    internal PetroglyphXmlLooseStringListParser(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override IList<string> Parse(XElement element)
    {
        var trimmedValued = element.Value.Trim();

        if (trimmedValued.Length == 0)
            return Array.Empty<string>();

        if (trimmedValued.Length > 0x2000)
        {
            Logger?.LogWarning($"Input value is too long '{trimmedValued.Length}' at {XmlLocationInfo.FromElement(element)}");
            return Array.Empty<string>();
        }

        var entries = trimmedValued.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

        return entries;
    }
}