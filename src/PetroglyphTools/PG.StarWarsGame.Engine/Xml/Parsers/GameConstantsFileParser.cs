using System;
using System.Xml.Linq;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

public class GameConstantsFileParser(IServiceProvider serviceProvider) : PetroglyphXmlFileParser<GameConstants>(serviceProvider)
{
    public override GameConstants Parse(XElement element)
    {
        return new GameConstants();
    }
}