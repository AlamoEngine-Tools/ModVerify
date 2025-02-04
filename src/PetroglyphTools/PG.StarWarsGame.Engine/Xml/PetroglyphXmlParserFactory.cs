using System;
using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.GameConstants;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Engine.GuiDialog.Xml;
using PG.StarWarsGame.Engine.Xml.Parsers.Data;
using PG.StarWarsGame.Engine.Xml.Parsers.File;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;
using PG.StarWarsGame.Files.XML.Parsers.Primitives;

namespace PG.StarWarsGame.Engine.Xml;

internal sealed class PetroglyphXmlFileParserFactory(IServiceProvider serviceProvider) : IPetroglyphXmlFileParserFactory
{
    public IPetroglyphXmlFileParser<T> CreateFileParser<T>(IXmlParserErrorListener? listener = null)
    {
        return (IPetroglyphXmlFileParser<T>)GetFileParser(typeof(T), listener);
    }

    private IPetroglyphXmlFileParser GetFileParser(Type type, IXmlParserErrorListener? listener)
    {
        if (type == typeof(XmlFileContainer))
            return new XmlFileContainerParser(serviceProvider, listener);

        if (type == typeof(GameConstantsXml))
            return new GameConstantsParser(serviceProvider, listener);

        if (type == typeof(GuiDialogsXml))
            return new GuiDialogParser(serviceProvider, listener);

        if (type == typeof(GameObject))
            return new GameObjectFileFileParser(serviceProvider, listener);

        if (type == typeof(SfxEvent))
            return new SfxEventFileParser(serviceProvider, listener);

        if (type == typeof(CommandBarComponentData))
            return new CommandBarComponentFileParser(serviceProvider, listener);

        throw new ParserNotFoundException(type);
    }
}