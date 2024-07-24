using System;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Xml.Parsers.Data;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.File;

internal class SfxEventFileParser(IServiceProvider serviceProvider, IXmlParserErrorListener? listener = null) 
    : PetroglyphXmlFileParser<SfxEvent>(serviceProvider, listener)
{
    private readonly IXmlParserErrorListener? _listener = listener;

    protected override void Parse(XElement element, IValueListDictionary<Crc32, SfxEvent> parsedElements)
    {
        var parser = new SfxEventParser(parsedElements, ServiceProvider, _listener);

        if (!element.HasElements)
        {
            OnParseError(XmlParseErrorEventArgs.FromEmptyRoot(XmlLocationInfo.FromElement(element).XmlFile, element));
            return;
        }

        foreach (var xElement in element.Elements())
        {
            var sfxEvent = parser.Parse(xElement, out var nameCrc);
            if (nameCrc == default)
            {
                var location = XmlLocationInfo.FromElement(xElement);
                OnParseError(new XmlParseErrorEventArgs(location.XmlFile, xElement, XmlParseErrorKind.MissingAttribute,
                    $"SFXEvent has no name at location '{location}'"));
            }

            parsedElements.Add(nameCrc, sfxEvent);
        }

    }

    protected override void OnParseError(XmlParseErrorEventArgs e)
    {
        Logger?.LogWarning($"Error while parsing {e.File}: {e.Message}");
        base.OnParseError(e);
    }

    public override SfxEvent Parse(XElement element)
    {
        throw new NotSupportedException();
    }
}