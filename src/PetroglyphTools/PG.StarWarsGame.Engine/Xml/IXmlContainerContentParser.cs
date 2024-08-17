﻿using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Repositories;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Xml;

internal interface IXmlContainerContentParser
{
    void ParseEntriesFromContainerXml<T>(
        string xmlFile,
        IXmlParserErrorListener listener,
        IGameRepository gameRepository,
        ValueListDictionary<Crc32, T> entries);
}