using System;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Xml.Parsers.Data;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.File;

internal class SfxEventFileParser(IServiceProvider serviceProvider) 
    : PetroglyphXmlFileParser<SfxEvent>(serviceProvider)
{
    protected override void Parse(XElement element, IValueListDictionary<Crc32, SfxEvent> parsedElements)
    {
        var parser = new SfxEventParser(parsedElements, ServiceProvider);

        try
        {
            parser.ParseError += OnInnerParseError;
            if (!element.HasElements)
            {
                OnParseError(ParseErrorEventArgs.FromEmptyRoot(XmlLocationInfo.FromElement(element).XmlFile, element));
                return;
            }

            foreach (var xElement in element.Elements())
            {
                var sfxEvent = parser.Parse(xElement, out var nameCrc);
                if (nameCrc == default)
                {
                    var location = XmlLocationInfo.FromElement(xElement);
                    OnParseError(new ParseErrorEventArgs(location.XmlFile, xElement, XmlParseErrorKind.MissingAttribute,
                        $"SFXEvent has no name at location '{location}'"));
                }
                parsedElements.Add(nameCrc, sfxEvent);
            }
        }
        finally
        {
            parser.ParseError -= OnInnerParseError;
        }
    }

    protected override void OnParseError(ParseErrorEventArgs e)
    {
        Logger?.LogWarning($"Error while parsing {e.File}: {e.Message}");
        base.OnParseError(e);
    }
    
    private void OnInnerParseError(object sender, ParseErrorEventArgs e)
    {
        OnParseError(e);
    }

    public override SfxEvent Parse(XElement element)
    {
        throw new NotSupportedException();
    }
}