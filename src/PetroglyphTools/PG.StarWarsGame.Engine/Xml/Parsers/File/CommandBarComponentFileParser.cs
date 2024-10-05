using System;
using System.Xml.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.Xml.Parsers.Data;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.File;

internal class CommandBarComponentFileParser(IServiceProvider serviceProvider, IXmlParserErrorListener? listener = null) 
    : PetroglyphXmlFileParser<CommandBarComponentData>(serviceProvider, listener)
{
    private readonly IXmlParserErrorListener? _listener = listener;

    protected override void Parse(XElement element, IValueListDictionary<Crc32, CommandBarComponentData> parsedElements, string fileName)
    {
        var parser = new CommandBarComponentParser(parsedElements, ServiceProvider, _listener);

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

    protected override CommandBarComponentData Parse(XElement element, string fileName)
    {
        throw new NotSupportedException();
    }
}