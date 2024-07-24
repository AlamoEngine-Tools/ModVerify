using System;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Xml.Parsers.Data;
using PG.StarWarsGame.Engine.Xml.Parsers.File;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;
using PG.StarWarsGame.Files.XML.Parsers.Primitives;

namespace PG.StarWarsGame.Engine.Xml;

internal sealed class PetroglyphXmlFileParserFactory(IServiceProvider serviceProvider) : IPetroglyphXmlFileParserFactory
{
    public IPetroglyphXmlFileParser<T> GetFileParser<T>(IXmlParserErrorListener? listener = null)
    {
        return (IPetroglyphXmlFileParser<T>)GetFileParser(typeof(T), listener);
    }

    private IPetroglyphXmlFileParser GetFileParser(Type type, IXmlParserErrorListener? listener)
    {
        if (type == typeof(XmlFileContainer))
            return new XmlFileContainerParser(serviceProvider, listener);

        if (type == typeof(GameConstants))
            return new GameConstantsParser(serviceProvider, listener);

        if (type == typeof(GameObject))
            return new GameObjectFileFileParser(serviceProvider, listener);

        if (type == typeof(SfxEvent))
            return new SfxEventFileParser(serviceProvider, listener);

        throw new ParserNotFoundException(type);
    }
}