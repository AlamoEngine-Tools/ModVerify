using System;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Xml.Parsers.Data;
using PG.StarWarsGame.Engine.Xml.Parsers.File;
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

    private IPetroglyphXmlFileParser GetFileParser(Type type)
    {
        if (type == typeof(XmlFileContainer))
            return new XmlFileContainerParser(serviceProvider);

        if (type == typeof(GameConstants))
            return new GameConstantsParser(serviceProvider);

        if (type == typeof(GameObject))
            return new GameObjectFileFileParser(serviceProvider);

        if (type == typeof(SfxEvent))
            return new SfxEventFileParser(serviceProvider);

        throw new ParserNotFoundException(type);
    }
}