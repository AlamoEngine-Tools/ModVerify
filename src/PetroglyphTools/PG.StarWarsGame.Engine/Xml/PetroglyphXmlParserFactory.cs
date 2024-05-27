using System;
using System.Collections.Generic;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Xml.Parsers;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Parsers;
using PG.StarWarsGame.Files.XML.Parsers.Primitives;

namespace PG.StarWarsGame.Engine.Xml;

internal sealed class PetroglyphXmlFileParserFactory(IServiceProvider serviceProvider) : IPetroglyphXmlFileParserFactory
{
    public IPetroglyphXmlFileParser<T> GetFileParser<T>()
    {
        return (IPetroglyphXmlFileParser<T>)GetFileParser(typeof(T));
    }

    public IPetroglyphXmlFileParser GetFileParser(Type type)
    {
        if (type == typeof(XmlFileContainer))
            return new XmlFileContainerParser(serviceProvider);

        if (type == typeof(GameConstants))
            return new GameConstantsFileParser(serviceProvider);

        if (type == typeof(IList<GameObject>))
            return new GameObjectFileFileParser(serviceProvider);

        throw new NotImplementedException($"The parser for the type {type} is not yet implemented.");
    }
}