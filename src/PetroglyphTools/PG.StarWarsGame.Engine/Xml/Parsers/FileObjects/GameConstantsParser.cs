using System;
using System.Xml.Linq;
using PG.StarWarsGame.Engine.GameConstants;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal class GameConstantsParser(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter = null) :
    XmlFileParser<GameConstantsXml>(serviceProvider, errorReporter)
{
    protected override GameConstantsXml ParseRoot(XElement element, string fileName)
    {
        return new GameConstantsXml(new XmlLocationInfo(fileName, null));
    }
}