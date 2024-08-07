﻿using System;
using System.Xml.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

internal class GameConstantsParser(IServiceProvider serviceProvider, IXmlParserErrorListener? listener = null) :
    PetroglyphXmlFileParser<GameConstants>(serviceProvider, listener)
{
    public override GameConstants Parse(XElement element)
    {
        return new GameConstants();
    }

    protected override void Parse(XElement element, IValueListDictionary<Crc32, GameConstants> parsedElements)
    {
        throw new NotSupportedException();
    }
}