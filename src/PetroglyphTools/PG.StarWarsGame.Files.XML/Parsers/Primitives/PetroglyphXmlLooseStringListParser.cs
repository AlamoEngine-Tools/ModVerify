﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

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
    
    internal PetroglyphXmlLooseStringListParser(IServiceProvider serviceProvider, IPrimitiveXmlParserErrorListener listener) : base(serviceProvider, listener)
    {
    }

    public override IList<string> Parse(XElement element)
    {
        var trimmedValued = element.Value.Trim();

        if (trimmedValued.Length == 0)
            return Array.Empty<string>();

        if (trimmedValued.Length > 0x2000)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.TooLongData,
                $"Input value is too long '{trimmedValued.Length}' at {XmlLocationInfo.FromElement(element)}"));

            return Array.Empty<string>();
        }

        var entries = trimmedValued.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

        return entries;
    }

    protected override void OnParseError(XmlParseErrorEventArgs e)
    {
        Logger?.LogWarning(e.Message);
        base.OnParseError(e);
    }
}