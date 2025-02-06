using System;
using System.Xml.Linq;
using PG.StarWarsGame.Engine.GameConstants;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

internal class GameConstantsParser(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter = null) :
    PetroglyphXmlFileParser<GameConstantsXml>(serviceProvider, errorReporter)
{
    protected override GameConstantsXml Parse(XElement element, string fileName)
    {
        return new GameConstantsXml();
    }
}