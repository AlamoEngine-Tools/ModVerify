using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

public class GameObjectFileFileParser(IServiceProvider serviceProvider) : PetroglyphXmlFileParser<IList<GameObject>>(serviceProvider)
{
    protected override bool LoadLineInfo => true;

    public override IList<GameObject> Parse(XElement element)
    {
        var parser = new GameObjectParser(ServiceProvider);
        return element.Elements().Select(parser.Parse).ToList();
    }
}