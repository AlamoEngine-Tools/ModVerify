using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Engine.Xml.Parsers.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;
using System;

namespace PG.StarWarsGame.Engine.Xml;

internal sealed class PetroglyphXmlFileParserFactory(IServiceProvider serviceProvider) : IPetroglyphXmlFileParserFactory
{
    public IPetroglyphXmlFileContainerParser<T> CreateFileContainerParser<T>(IXmlParserErrorReporter? errorReporter) where T : XmlObject
    {
        if (typeof(T) == typeof(SfxEvent))
            return new PetroglyphXmlFileContainerParser<T>(
                serviceProvider, (IPetroglyphXmlNamedElementParser<T>)new SfxEventParser(serviceProvider, errorReporter),
                errorReporter);

        if (typeof(T) == typeof(CommandBarComponentData))
            return new PetroglyphXmlFileContainerParser<T>(
                serviceProvider, (IPetroglyphXmlNamedElementParser<T>)new CommandBarComponentParser(serviceProvider, errorReporter),
                errorReporter);

        if (typeof(T) == typeof(GameObject))
            return new PetroglyphXmlFileContainerParser<T>(
                serviceProvider, (IPetroglyphXmlNamedElementParser<T>)new GameObjectParser(serviceProvider, errorReporter),
                errorReporter);


        throw new ParserNotFoundException(typeof(T));
    }
}