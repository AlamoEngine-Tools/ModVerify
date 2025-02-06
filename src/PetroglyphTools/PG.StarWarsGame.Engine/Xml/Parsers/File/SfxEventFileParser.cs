using System;
using System.Xml.Linq;
using PG.Commons.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.Xml.Parsers.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.File;

internal class SfxEventFileParser(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter = null) 
    : PetroglyphXmlFileContainerParser<SfxEvent>(serviceProvider, errorReporter)
{
  protected override void Parse(XElement element, IValueListDictionary<Crc32, SfxEvent> parsedElements, string fileName)
    {
        var parser = new SfxEventParser(parsedElements, ServiceProvider, ErrorReporter);

        if (!element.HasElements)
        {
            OnParseError(XmlParseErrorEventArgs.FromEmptyRoot(element));
            return;
        }

        foreach (var xElement in element.Elements())
        {
            var sfxEvent = parser.Parse(xElement, out var nameCrc);
            parsedElements.Add(nameCrc, sfxEvent);
        }

    }
}