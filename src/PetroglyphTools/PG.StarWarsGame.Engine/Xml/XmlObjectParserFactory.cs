using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Engine.Xml.Parsers;

namespace PG.StarWarsGame.Engine.Xml;

internal sealed class XmlObjectParserFactory(IServiceProvider serviceProvider) : IPetroglyphXmlFileParserFactory
{
    public NamedXmlObjectParser<T> CreateNamedXmlObjectParser<T>(GameEngineType engine, IXmlParserErrorReporter? errorReporter) where T : NamedXmlObject
    {
        if (typeof(T) == typeof(SfxEvent))
            return ChangeType<T>(new SfxEventParser(engine, serviceProvider, errorReporter));
        if (typeof(T) == typeof(CommandBarComponentData))
            return ChangeType<T>(new CommandBarComponentParser(engine, serviceProvider, errorReporter));
        if (typeof(T) == typeof(GameObjectType))
            return ChangeType<T>(new GameObjectTypeParser(engine, serviceProvider, errorReporter));


        throw new ParserNotFoundException(typeof(T));
    }

    private static NamedXmlObjectParser<T> ChangeType<T>(object obj) where T : NamedXmlObject
    {
        return (NamedXmlObjectParser<T>) obj;
    }
}