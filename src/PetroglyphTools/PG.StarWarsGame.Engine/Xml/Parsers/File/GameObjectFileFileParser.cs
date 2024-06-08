using System;
using System.Xml.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Xml.Parsers.Data;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.File;

internal class GameObjectFileFileParser(IServiceProvider serviceProvider) 
    : PetroglyphXmlFileParser<GameObject>(serviceProvider)
{
    protected override void Parse(XElement element, IValueListDictionary<Crc32, GameObject> parsedElements)
    {
        var parser = new GameObjectParser(ServiceProvider);

        foreach (var xElement in element.Elements())
        {
            var sfxEvent = parser.Parse(xElement, parsedElements, out var nameCrc);
            parsedElements.Add(nameCrc, sfxEvent);
        }
    }

    public override GameObject Parse(XElement element)
    {
        throw new NotSupportedException();
    }
}