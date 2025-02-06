using System;
using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Engine.Xml.Parsers.File;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml;

internal sealed class PetroglyphXmlFileParserFactory(IServiceProvider serviceProvider) : IPetroglyphXmlFileParserFactory
{
    public IPetroglyphXmlFileContainerParser<T> CreateFileParser<T>(IXmlParserErrorReporter? errorReporter) where T : notnull
    {
        if (typeof(T) == typeof(SfxEvent))
            return (IPetroglyphXmlFileContainerParser<T>) new SfxEventFileParser(serviceProvider, errorReporter);

        if (typeof(T) == typeof(CommandBarComponentData))
            return (IPetroglyphXmlFileContainerParser<T>)new CommandBarComponentFileParser(serviceProvider, errorReporter);

        if (typeof(T) == typeof(GameObject))
            return (IPetroglyphXmlFileContainerParser<T>)new GameObjectFileParser(serviceProvider, errorReporter);


        throw new NotImplementedException($"Unable to get parser for type {typeof(T)}");
    }
}