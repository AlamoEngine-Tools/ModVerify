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
        foreach (var xElement in element.Elements())
        {
            var sfxEvent = parser.Parse(xElement, out var nameCrc);
            if (nameCrc == default)
                Logger?.LogWarning($"SFXEvent has no name at location '{XmlLocationInfo.FromElement(xElement)}'");
            parsedElements.Add(nameCrc, sfxEvent);
        }
    }

    public override SfxEvent Parse(XElement element)
    {
        throw new NotSupportedException();
    }
}