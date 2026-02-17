using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal sealed class PetroglyphXmlFileParserFactory(IServiceProvider serviceProvider) : IPetroglyphXmlFileParserFactory
{
    public NamedXmlObjectParser<T> CreateNamedXmlObjectParser<T>(IXmlParserErrorReporter? errorReporter) where T : NamedXmlObject
    {
        if (typeof(T) == typeof(SfxEvent))
            return ChangeType<T>(new SfxEventParser(serviceProvider, errorReporter));
        if (typeof(T) == typeof(CommandBarComponentData))
            return ChangeType<T>(new CommandBarComponentParser(serviceProvider, errorReporter));
        if (typeof(T) == typeof(GameObject))
            return ChangeType<T>(new GameObjectParser(serviceProvider, errorReporter));


        throw new ParserNotFoundException(typeof(T));
    }

    private static NamedXmlObjectParser<T> ChangeType<T>(object obj) where T : NamedXmlObject
    {
        return (NamedXmlObjectParser<T>) obj;
    }
}