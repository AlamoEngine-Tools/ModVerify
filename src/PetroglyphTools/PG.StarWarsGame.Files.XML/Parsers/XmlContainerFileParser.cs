using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using System.IO;
using PG.StarWarsGame.Files.XML.Data;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class XmlContainerFileParser<T>(
    IServiceProvider serviceProvider,
    NamedXmlObjectParser<T> elementParser,
    IXmlParserErrorReporter? listener = null)
    : PetroglyphXmlFileParserBase(serviceProvider, listener) where T : NamedXmlObject
{
    public NamedXmlObjectParser<T> ElementParser { get; } =
        elementParser ?? throw new ArgumentNullException(nameof(elementParser));

    public void ParseFile(Stream xmlStream, IFrugalValueListDictionary<Crc32, T> parsedEntries)
    {
        var root = GetRootElement(xmlStream, out _);
        
        foreach (var xElement in root.Elements())
        {
            var parsedElement = ElementParser.Parse(xElement, parsedEntries, out var entryCrc);
            parsedEntries.Add(entryCrc, parsedElement);
        }
    }
}