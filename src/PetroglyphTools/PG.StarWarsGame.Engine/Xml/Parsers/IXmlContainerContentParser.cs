using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using PG.StarWarsGame.Engine.IO.Repositories;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal interface IXmlContainerContentParser
{
    event EventHandler<XmlContainerParserErrorEventArgs>? XmlParseError;

    void ParseEntriesFromContainerXml<T>(
        string xmlFile,
        IXmlParserErrorListener listener,
        IGameRepository gameRepository,
        string lookupPath,
        ValueListDictionary<Crc32, T> entries,
        Action<string>? onFileParseAction = null);
}