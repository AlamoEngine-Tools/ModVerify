using System;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml;

public interface IPetroglyphXmlFileParserFactory
{
    IPetroglyphXmlFileParser<T> GetFileParser<T>();

    IPetroglyphXmlFileParser GetFileParser(Type type);
}