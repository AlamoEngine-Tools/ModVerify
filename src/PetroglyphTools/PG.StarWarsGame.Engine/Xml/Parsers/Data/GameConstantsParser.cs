using System;
using System.Xml.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.GameConstants;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

internal class GameConstantsParser(IServiceProvider serviceProvider, IXmlParserErrorListener? listener = null) :
    PetroglyphXmlFileParser<GameConstantsXml>(serviceProvider, listener)
{
    protected override GameConstantsXml Parse(XElement element, string fileName)
    {
        return new GameConstantsXml();
    }

    protected override void Parse(XElement element, IValueListDictionary<Crc32, GameConstantsXml> parsedElements, string fileName)
    {
        throw new NotSupportedException();
    }
}