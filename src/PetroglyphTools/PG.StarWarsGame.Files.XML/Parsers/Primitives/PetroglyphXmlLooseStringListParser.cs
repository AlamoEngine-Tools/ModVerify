using System;
using System.Collections.Generic;
using System.Xml.Linq;

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

        var entries = trimmedValued.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

        return entries;
    }
}